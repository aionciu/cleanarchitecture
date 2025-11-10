using System.Net;
using Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware;

/// <summary>
/// Middleware to handle exceptions globally and return consistent error responses using ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="environment">The web host environment.</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Get correlation ID from context if available
        var correlationId = context.Items.TryGetValue("CorrelationId", out var id) 
            ? id?.ToString() ?? context.TraceIdentifier 
            : context.TraceIdentifier;
        
        // Log the exception with full details
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, QueryString: {QueryString}",
            correlationId,
            context.Request.Path,
            context.Request.Method,
            context.Request.QueryString);

        // Create ProblemDetails based on exception type
        // Note: More specific exceptions must be checked before their base types
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, correlationId, requestPath),
            NotFoundException notFoundEx => CreateNotFoundProblemDetails(notFoundEx, correlationId, requestPath),
            DbUpdateConcurrencyException concurrencyEx => CreateConcurrencyProblemDetails(concurrencyEx, correlationId, requestPath),
            ArgumentNullException argNullEx => CreateBadRequestProblemDetails(argNullEx, correlationId, requestPath),
            ArgumentException argEx => CreateBadRequestProblemDetails(argEx, correlationId, requestPath),
            DbUpdateException dbUpdateEx => CreateDatabaseProblemDetails(dbUpdateEx, correlationId, requestPath),
            _ => CreateInternalServerProblemDetails(exception, correlationId, requestPath)
        };

        // Set response status code
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        
        // Write response with ProblemDetails content type
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json");
    }

    private static ProblemDetails CreateValidationProblemDetails(ValidationException exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["errors"] = exception.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())
            }
        };

        return problemDetails;
    }

    private static ProblemDetails CreateNotFoundProblemDetails(NotFoundException exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Resource Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Detail = exception.Message,
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        if (!string.IsNullOrEmpty(exception.ResourceName))
        {
            problemDetails.Extensions["resourceName"] = exception.ResourceName;
        }

        if (!string.IsNullOrEmpty(exception.Identifier))
        {
            problemDetails.Extensions["identifier"] = exception.Identifier;
        }

        return problemDetails;
    }

    private static ProblemDetails CreateConcurrencyProblemDetails(DbUpdateConcurrencyException exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Concurrency Conflict",
            Status = (int)HttpStatusCode.Conflict,
            Detail = "The resource has been modified by another operation. Please refresh and try again.",
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        return problemDetails;
    }

    private ProblemDetails CreateDatabaseProblemDetails(DbUpdateException exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Database Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = _environment.IsDevelopment()
                ? exception.GetBaseException().Message
                : "An error occurred while processing your request.",
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        return problemDetails;
    }

    private static ProblemDetails CreateBadRequestProblemDetails(ArgumentException exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = exception.Message,
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        if (!string.IsNullOrEmpty(exception.ParamName))
        {
            problemDetails.Extensions["paramName"] = exception.ParamName;
        }

        return problemDetails;
    }

    private ProblemDetails CreateInternalServerProblemDetails(Exception exception, string correlationId, string instance)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = _environment.IsDevelopment()
                ? exception.GetBaseException().Message
                : "An error occurred while processing your request.",
            Instance = instance,
            Extensions =
            {
                ["correlationId"] = correlationId
            }
        };

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        return problemDetails;
    }
}

