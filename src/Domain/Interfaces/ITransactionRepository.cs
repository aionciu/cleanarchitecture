using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repository interface for transaction persistence operations.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adds a new transaction to the repository.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created transaction with generated Id and timestamps.</returns>
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}


