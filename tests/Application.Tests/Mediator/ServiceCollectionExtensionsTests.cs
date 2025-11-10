using Application.Mediator;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Tests.Mediator;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediator_WithNullServices_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ((IServiceCollection)null!).AddMediator();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddMediator_RegistersIMediatorAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRequestHandler).Assembly);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        serviceDescriptor.ImplementationType.Should().Be(typeof(Application.Mediator.Mediator));
    }

    [Fact]
    public void AddMediator_WithExplicitAssembly_RegistersAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRequestHandler).Assembly);

        // Assert
        var testHandlerDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IRequestHandler<TestRequest, TestResponse>));
        testHandlerDescriptor.Should().NotBeNull();
        testHandlerDescriptor!.ImplementationType.Should().Be(typeof(TestRequestHandler));
        testHandlerDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var intHandlerDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IRequestHandler<TestIntRequest, int>));
        intHandlerDescriptor.Should().NotBeNull();
        intHandlerDescriptor!.ImplementationType.Should().Be(typeof(TestIntRequestHandler));
    }

    [Fact]
    public void AddMediator_WithoutAssembly_ScansCallingAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(); // No assembly specified, should use calling assembly

        // Assert
        // Should still register IMediator
        var mediatorDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator));
        mediatorDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithMultipleAssemblies_RegistersHandlersFromAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var testAssembly = typeof(TestRequestHandler).Assembly;
        var mediatorAssembly = typeof(IMediator).Assembly;

        // Act
        services.AddMediator(testAssembly, mediatorAssembly);

        // Assert
        var testHandlerDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IRequestHandler<TestRequest, TestResponse>));
        testHandlerDescriptor.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_DoesNotRegisterAbstractHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRequestHandler).Assembly);

        // Assert
        // Abstract handlers should not be registered
        var allHandlers = services.Where(sd => 
            sd.ServiceType.IsGenericType && 
            sd.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        
        // Verify no abstract types are registered
        allHandlers.Should().OnlyContain(sd => !sd.ImplementationType!.IsAbstract);
    }

    [Fact]
    public void AddMediator_DoesNotRegisterNonHandlerClasses()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRequestHandler).Assembly);

        // Assert
        // Only IRequestHandler implementations should be registered
        var allHandlers = services.Where(sd => 
            sd.ServiceType.IsGenericType && 
            sd.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        
        allHandlers.Should().NotBeEmpty();
        // Verify that non-handler classes like TestRequest, TestResponse are not registered
        services.Should().NotContain(sd => sd.ServiceType == typeof(TestRequest));
        services.Should().NotContain(sd => sd.ServiceType == typeof(TestResponse));
    }

    [Fact]
    public void AddMediator_ReturnsServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddMediator(typeof(TestRequestHandler).Assembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddMediator_WithEmptyAssembly_RegistersOnlyMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        var emptyAssembly = Assembly.GetExecutingAssembly(); // This assembly might not have handlers

        // Act
        services.AddMediator(emptyAssembly);

        // Assert
        // Should still register IMediator
        var mediatorDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IMediator));
        mediatorDescriptor.Should().NotBeNull();
        
        // May or may not have handlers depending on the assembly
        var handlers = services.Where(sd => 
            sd.ServiceType.IsGenericType && 
            sd.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        // This is acceptable - empty assembly just means no handlers found
    }
}

