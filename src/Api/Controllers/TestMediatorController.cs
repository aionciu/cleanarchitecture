using Application.Commands.TestMediator;
using Application.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestMediatorController : ControllerBase
{
    private readonly IMediator _mediator;

    public TestMediatorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Test endpoint to verify MediatR is working correctly.
    /// </summary>
    /// <param name="message">Optional message to include in the response</param>
    /// <returns>Test response from MediatR handler</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TestMediatorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TestMediatorResponse>> TestMediatR([FromQuery] string? message = null)
    {
        var command = new TestMediatorCommand
        {
            Message = message ?? "Hello from MediatR!"
        };

        var response = await _mediator.Send(command);

        return Ok(response);
    }

    /// <summary>
    /// Test endpoint with POST to verify MediatR is working correctly.
    /// </summary>
    /// <param name="command">Test command with message</param>
    /// <returns>Test response from MediatR handler</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TestMediatorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TestMediatorResponse>> TestMediatRPost([FromBody] TestMediatorCommand command)
    {
        var response = await _mediator.Send(command);

        return Ok(response);
    }
}




