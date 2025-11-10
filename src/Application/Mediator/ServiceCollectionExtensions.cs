using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Mediator;

/// <summary>
/// Extension methods for registering Mediator services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Mediator and all request handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for handlers. If not provided, scans the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Register IMediator with Mediator implementation
        services.AddScoped<IMediator, Mediator>();

        // If no assemblies provided, use the calling assembly
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Scan assemblies for handlers
        var handlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { HandlerType = type, RequestType = i.GetGenericArguments()[0], ResponseType = i.GetGenericArguments()[1] }))
            .ToList();

        // Register each handler
        foreach (var handler in handlerTypes)
        {
            var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(handler.RequestType, handler.ResponseType);
            services.AddScoped(handlerInterface, handler.HandlerType);
        }

        return services;
    }
}




