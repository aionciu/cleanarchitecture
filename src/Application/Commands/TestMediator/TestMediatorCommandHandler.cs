using Application.Mediator;

namespace Application.Commands.TestMediator;

public class TestMediatorCommandHandler : IRequestHandler<TestMediatorCommand, TestMediatorResponse>
{
    public Task<TestMediatorResponse> HandleAsync(TestMediatorCommand request, CancellationToken cancellationToken = default)
    {
        var response = new TestMediatorResponse
        {
            Result = $"MediatR is working! Received message: '{request.Message}'",
            ProcessedAt = DateTime.UtcNow,
            Success = true
        };

        return Task.FromResult(response);
    }
}




