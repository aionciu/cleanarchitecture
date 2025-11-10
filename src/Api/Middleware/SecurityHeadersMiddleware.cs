using Microsoft.AspNetCore.Hosting;

namespace Api.Middleware;

/// <summary>
/// Middleware to add security headers to all responses.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="environment">The web host environment.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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

        // Skip security headers for Swagger and metrics endpoints in development to avoid CSP issues
        // Metrics endpoint needs plain text response without security headers
        if ((isSwaggerPath || isMetricsPath) && _environment.IsDevelopment())
        {
            await _next(context);
            return;
        }

        // Add security headers
        var response = context.Response;

        // Prevent MIME type sniffing
        response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking
        response.Headers.Append("X-Frame-Options", "DENY");

        // XSS Protection (legacy but still useful)
        response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // HSTS - Only add if using HTTPS
        if (context.Request.IsHttps)
        {
            response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        // Content Security Policy - Strict for production
        response.Headers.Append("Content-Security-Policy", "default-src 'self'");

        // Remove Server header (done via Kestrel configuration, but ensure it's removed)
        response.Headers.Remove("Server");

        await _next(context);
    }
}

