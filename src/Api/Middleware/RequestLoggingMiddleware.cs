using System.Diagnostics;
using System.Text;

namespace Api.Middleware;

/// <summary>
/// Middleware to log HTTP requests and responses with correlation IDs and performance metrics.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var isSwaggerPath = path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
        var isMetricsPath = path.Equals("/metrics", StringComparison.OrdinalIgnoreCase);
        var isStaticPath = isSwaggerPath || isMetricsPath;

        // Generate or retrieve correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        // Start stopwatch for duration measurement
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        // Log request details
        LogRequest(context, correlationId, startTime);

        // For Swagger and metrics paths, don't intercept the response body stream
        // These endpoints serve static files/plain text that shouldn't be buffered
        if (isStaticPath)
        {
            try
            {
                await _next(context);
                stopwatch.Stop();
                LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds, startTime);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Request processing failed. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, Duration: {DurationMs}ms",
                    correlationId,
                    context.Request.Path,
                    context.Request.Method,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            return;
        }

        // For non-Swagger paths, capture response body for logging
        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response details
            LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds, startTime);

            // Copy response body back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Log error with correlation ID
            _logger.LogError(
                ex,
                "Request processing failed. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, Duration: {DurationMs}ms",
                correlationId,
                context.Request.Path,
                context.Request.Method,
                stopwatch.ElapsedMilliseconds);

            // Re-throw to let ExceptionHandlingMiddleware handle it
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check if correlation ID is provided in request headers
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationIdHeader) &&
            !string.IsNullOrWhiteSpace(correlationIdHeader))
        {
            return correlationIdHeader.ToString();
        }

        // Check if it's already in the trace identifier
        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString();
    }

    private void LogRequest(HttpContext context, string correlationId, DateTime timestamp)
    {
        var request = context.Request;
        var clientCertificate = context.Connection.ClientCertificate;
        var clientCertificateThumbprint = clientCertificate?.Thumbprint ?? "N/A";

        // Build query string (sanitized if needed)
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

        // Get client IP address
        var clientIp = GetClientIpAddress(context);

        // Log request with structured logging
        _logger.LogInformation(
            "Incoming HTTP request. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, QueryString: {QueryString}, ClientIP: {ClientIP}, ClientCertificate: {ClientCertificate}, Timestamp: {Timestamp}",
            correlationId,
            request.Method,
            request.Path,
            queryString,
            clientIp,
            clientCertificateThumbprint,
            timestamp);
    }

    private void LogResponse(HttpContext context, string correlationId, long durationMs, DateTime startTime)
    {
        var request = context.Request;
        var response = context.Response;
        var endTime = DateTime.UtcNow;

        // Determine log level based on status code
        var logLevel = response.StatusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };

        // Log response with structured logging
        _logger.Log(
            logLevel,
            "HTTP response completed. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {DurationMs}ms, StartTime: {StartTime}, EndTime: {EndTime}",
            correlationId,
            request.Method,
            request.Path,
            response.StatusCode,
            durationMs,
            startTime,
            endTime);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (when behind proxy/load balancer)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(ip))
            {
                return ip;
            }
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(ip))
            {
                return ip;
            }
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

