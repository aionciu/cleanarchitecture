using Application;
using Application.Mediator;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Api.Extensions;

/// <summary>
/// Extension methods for configuring services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API services including controllers, Swagger, and API behavior configuration.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(ConfigureApiBehavior);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(ConfigureSwagger);

        return services;
    }

    /// <summary>
    /// Adds application layer services (Mediator, AutoMapper, FluentValidation).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediator(typeof(IMediator).Assembly);
        services.AddApplicationMappings();
        services.AddApplicationValidators();

        return services;
    }

    /// <summary>
    /// Configures API behavior options for consistent error responses.
    /// </summary>
    private static void ConfigureApiBehavior(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            problemDetails.Extensions["errors"] = errors;
            problemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation.
    /// </summary>
    private static void ConfigureSwagger(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        options.SupportNonNullableReferenceTypes();
        options.MapType<ProblemDetails>(() => CreateProblemDetailsSchema());
    }

    /// <summary>
    /// Creates the OpenAPI schema for ProblemDetails.
    /// </summary>
    private static OpenApiSchema CreateProblemDetailsSchema()
    {
        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = "string", Format = "uri" },
                ["title"] = new OpenApiSchema { Type = "string" },
                ["status"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                ["detail"] = new OpenApiSchema { Type = "string" },
                ["instance"] = new OpenApiSchema { Type = "string", Format = "uri" },
                ["errors"] = new OpenApiSchema
                {
                    Type = "object",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" }
                    }
                },
                ["correlationId"] = new OpenApiSchema { Type = "string" }
            },
            Required = new HashSet<string> { "type", "title", "status" }
        };
    }
}

