using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Mediator;

/// <summary>
/// Default mediator implementation relying on dependency injection for resolving handlers.
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, Type> _handlerTypes = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Asynchronously send a request to a single handler.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <param name="request">Request object.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();
        var handlerType = _handlerTypes.GetOrAdd(requestType, GetHandlerType);

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException(
                $"Handler was not found for request of type {requestType.Name}. " +
                $"Register your handlers with the service provider.");
        }

        var handleMethod = handlerType.GetMethod(
            "HandleAsync",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { requestType, typeof(CancellationToken) },
            null);
        if (handleMethod == null)
        {
            throw new InvalidOperationException(
                $"Handler for request type {requestType.Name} does not implement HandleAsync method.");
        }

        var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
        if (result is not Task<TResponse> task)
        {
            throw new InvalidOperationException(
                $"Handler for request type {requestType.Name} returned an unexpected result type.");
        }

        return task;
    }

    private static Type GetHandlerType(Type requestType)
    {
        var requestInterface = requestType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

        if (requestInterface == null)
        {
            throw new InvalidOperationException(
                $"Type {requestType.Name} does not implement IRequest<TResponse>.");
        }

        var responseType = requestInterface.GetGenericArguments()[0];
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        return handlerType;
    }
}

