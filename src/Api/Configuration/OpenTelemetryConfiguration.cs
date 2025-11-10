using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Configuration;

/// <summary>
/// Configuration for OpenTelemetry observability.
/// </summary>
public static class OpenTelemetryConfiguration
{
    private const string ServiceName = "CleanArchitecture.Api";
    private const string ServiceVersion = "1.0.0";

    /// <summary>
    /// Adds OpenTelemetry services with tracing, metrics, and logging.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => ConfigureResource(resource, environment))
            .WithTracing(ConfigureTracing)
            .WithMetrics(ConfigureMetrics);

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry logging.
    /// </summary>
    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
                .AddConsoleExporter();
        });

        return loggingBuilder;
    }

    /// <summary>
    /// Configures OpenTelemetry resource attributes.
    /// </summary>
    private static void ConfigureResource(ResourceBuilder resource, IWebHostEnvironment environment)
    {
        resource
            .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment.EnvironmentName
            });
    }

    /// <summary>
    /// Configures OpenTelemetry tracing.
    /// </summary>
    private static void ConfigureTracing(TracerProviderBuilder tracing)
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("http.request.body.size", request.ContentLength ?? 0);
                };
                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag("http.response.body.size", response.ContentLength ?? 0);
                };
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.SetTag("db.command.timeout", command.CommandTimeout);
                };
            })
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    }

    /// <summary>
    /// Configures OpenTelemetry metrics.
    /// </summary>
    private static void ConfigureMetrics(MeterProviderBuilder metrics)
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    }
}

