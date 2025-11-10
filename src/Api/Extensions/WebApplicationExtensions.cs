using Api.Middleware;

namespace Api.Extensions;

/// <summary>
/// Extension methods for configuring the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline with middleware in the correct order.
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Exception handling should be first to catch all exceptions
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // HTTPS redirection should be early (before Swagger)
        app.UseHttpsRedirection();

        // Swagger should be early in the pipeline (before custom middleware that modifies responses)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Enable OpenTelemetry Prometheus metrics endpoint early (before middleware that might interfere)
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        // Request logging should be after Swagger to avoid interfering with static files
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Security headers should be early in the pipeline
        app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}

