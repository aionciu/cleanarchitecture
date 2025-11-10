using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Repository implementation for transaction persistence operations.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly IDbContextFactory _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public TransactionRepository(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <summary>
    /// Adds a new transaction to the repository.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created transaction with generated Id and timestamps.</returns>
    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        using var context = _dbContextFactory.CreateDbContext();
        
        await context.Transactions.AddAsync(transaction, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return transaction;
    }
}

