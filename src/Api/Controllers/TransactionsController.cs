using Application.Commands.CreateTransaction;
using Application.DTOs;
using Application.Mediator;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for managing transactions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateTransactionCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="validator">The validator for CreateTransactionCommand.</param>
    public TransactionsController(
        IMediator mediator,
        IValidator<CreateTransactionCommand> validator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="command">The create transaction command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created transaction.</returns>
    /// <response code="201">Transaction created successfully.</response>
    /// <response code="400">Validation error or invalid request. Returns ProblemDetails.</response>
    /// <response code="500">Internal server error. Returns ProblemDetails.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        [FromBody] CreateTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Throw ValidationException to be handled by ExceptionHandlingMiddleware
            // This ensures consistent ProblemDetails format
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        // Send command through mediator
        var result = await _mediator.Send(command, cancellationToken);

        // Return 201 Created with Location header
        // Note: GetTransaction endpoint is not exposed, so we manually construct the Location header
        var locationUri = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/transactions/{result.Id}";
        return Created(new Uri(locationUri, UriKind.RelativeOrAbsolute), result);
    }
}

