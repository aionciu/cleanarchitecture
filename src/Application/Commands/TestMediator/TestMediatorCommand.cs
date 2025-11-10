using Application.Mediator;

namespace Application.Commands.TestMediator;

public class TestMediatorCommand : IRequest<TestMediatorResponse>
{
    public string Message { get; set; } = string.Empty;
}




