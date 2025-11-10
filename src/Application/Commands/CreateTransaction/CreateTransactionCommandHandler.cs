using Application.DTOs;
using Application.Mediator;
using AutoMapper;
using Domain.Interfaces;

namespace Application.Commands.CreateTransaction;

/// <summary>
/// Handler for CreateTransactionCommand.
/// </summary>
public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTransactionCommandHandler"/> class.
    /// </summary>
    /// <param name="transactionRepository">The transaction repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Handles the CreateTransactionCommand.
    /// </summary>
    /// <param name="request">The create transaction command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created transaction DTO.</returns>
    public async Task<TransactionDto> HandleAsync(CreateTransactionCommand request, CancellationToken cancellationToken = default)
    {
        // Map command to domain entity
        // The Transaction constructor will validate and set defaults (Status = Pending, Id, timestamps)
        var transaction = _mapper.Map<Domain.Entities.Transaction>(request);

        // Save to repository
        var createdTransaction = await _transactionRepository.AddAsync(transaction, cancellationToken);

        // Map entity to DTO
        var dto = _mapper.Map<TransactionDto>(createdTransaction);

        return dto;
    }
}


