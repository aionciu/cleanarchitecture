using Application.Mediator;

namespace Application.Tests.Mediator;

// Test request with response
public class TestRequest : IRequest<TestResponse>
{
    public string Message { get; set; } = string.Empty;
}

public class TestResponse
{
    public string Result { get; set; } = string.Empty;
}

// Test request with int response
public class TestIntRequest : IRequest<int>
{
    public int Value { get; set; }
}

// Test request that doesn't implement IRequest (for negative testing)
public class InvalidRequest
{
    public string Message { get; set; } = string.Empty;
}




