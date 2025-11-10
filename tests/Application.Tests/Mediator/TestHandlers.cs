using Application.Mediator;

namespace Application.Tests.Mediator;

public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TestResponse { Result = $"Processed: {request.Message}" });
    }
}

public class TestIntRequestHandler : IRequestHandler<TestIntRequest, int>
{
    public Task<int> HandleAsync(TestIntRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(request.Value * 2);
    }
}

// Handler that throws exception
public class ThrowingRequestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Handler exception");
    }
}

// Handler that checks cancellation token
public class CancellableRequestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new TestResponse { Result = "Not cancelled" });
    }
}




