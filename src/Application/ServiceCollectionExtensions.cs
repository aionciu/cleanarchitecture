using Application.Mappings;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

/// <summary>
/// Extension methods for service collection registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AutoMapper configuration to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TransactionMappingProfile).Assembly);
        return services;
    }

    /// <summary>
    /// Adds FluentValidation validators to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}


