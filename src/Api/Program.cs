using Api.Configuration;
using Api.Extensions;
using Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.CreateLogger(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

// Configure services
builder.Services
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructure(builder.Configuration)
    .AddOpenTelemetryServices(builder.Environment);

// Configure logging
builder.Logging.AddOpenTelemetryLogging();

// Configure Kestrel
builder.WebHost.ConfigureKestrelLimits();

// Build application
var app = builder.Build();

// Configure pipeline
app.ConfigurePipeline();

// Run application
try
{
    Log.Information("Starting CleanArchitecture API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
