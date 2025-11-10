using Serilog;

namespace Api.Configuration;

/// <summary>
/// Configuration for Serilog logging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures and creates the Serilog logger.
    /// </summary>
    public static LoggerConfiguration CreateLogger(IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Is(Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CleanArchitecture.Api")
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/cleanarchitecture.log",
                fileSizeLimitBytes: 5 * 1024 * 1024, // 5MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7, // Keep up to 7 rolled files, then delete oldest
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/errors/cleanarchitecture-errors.log",
                fileSizeLimitBytes: 5 * 1024 * 1024, // 5MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 30, // Keep error logs longer (30 files)
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error, // Only errors and above
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}{NewLine}");
    }
}

