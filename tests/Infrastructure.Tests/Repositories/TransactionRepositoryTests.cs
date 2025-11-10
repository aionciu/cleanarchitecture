using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly TransactionRepository _repository;

    private static readonly string ValidIban = "RO49AAAA1B31007593840000";
    private static readonly string ValidCurrency = "RON";
    private static readonly string ValidMerchantId = "MERCHANT-001";
    private static readonly string ValidCustomerId = "CUSTOMER-001";
    private static readonly decimal ValidAmount = 100.50m;
    private static readonly DateTime ValidTransactionDate = DateTime.UtcNow.AddDays(-1);

    public TransactionRepositoryTests()
    {
        // Use in-memory database for testing
        // Each test gets a fresh database
        _dbContextFactory = new InMemoryDbContextFactory();
        _repository = new TransactionRepository(_dbContextFactory);
    }

    [Fact]
    public async Task AddAsync_WithValidTransaction_ShouldSaveToDatabase()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(transaction, options => options
            .Excluding(t => t.Id)
            .Excluding(t => t.CreatedAt)
            .Excluding(t => t.UpdatedAt));

        // Verify it was saved to database
        using var verifyContext = _dbContextFactory.CreateDbContext();
        var savedTransaction = await verifyContext.Transactions.FindAsync(transaction.Id);
        savedTransaction.Should().NotBeNull();
        savedTransaction!.Amount.Should().Be(ValidAmount);
        savedTransaction.Currency.Should().Be(ValidCurrency);
        savedTransaction.Iban.Should().Be(ValidIban);
        savedTransaction.MerchantId.Should().Be(ValidMerchantId);
        savedTransaction.CustomerId.Should().Be(ValidCustomerId);
        savedTransaction.Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public async Task AddAsync_WithValidTransaction_ShouldGenerateId()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddAsync_WithValidTransaction_ShouldSetTimestamps()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        var result = await _repository.AddAsync(transaction);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeAfter(beforeCreation).And.BeBefore(afterCreation.AddSeconds(1));
        result.UpdatedAt.Should().BeAfter(beforeCreation).And.BeBefore(afterCreation.AddSeconds(1));
        result.CreatedAt.Should().BeCloseTo(result.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddAsync_WithValidTransaction_ShouldSetStatusToPending()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public async Task AddAsync_WithNullTransaction_ShouldThrowArgumentNullException()
    {
        // Arrange
        Transaction? transaction = null;

        // Act
        var act = async () => await _repository.AddAsync(transaction!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("transaction");
    }

    [Fact]
    public async Task AddAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var cancellationToken = new CancellationToken();

        // Act
        var result = await _repository.AddAsync(transaction, cancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var act = async () => await _repository.AddAsync(transaction, cancellationTokenSource.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AddAsync_WithMultipleTransactions_ShouldSaveAll()
    {
        // Arrange
        var transaction1 = new Transaction(
            100.50m,
            "RON",
            "RO49AAAA1B31007593840000",
            "MERCHANT-001",
            "CUSTOMER-001",
            DateTime.UtcNow.AddDays(-1));

        var transaction2 = new Transaction(
            250.75m,
            "EUR",
            "GB82WEST12345698765432",
            "MERCHANT-002",
            "CUSTOMER-002",
            DateTime.UtcNow.AddDays(-2));

        // Act
        var result1 = await _repository.AddAsync(transaction1);
        var result2 = await _repository.AddAsync(transaction2);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Id.Should().NotBe(result2.Id);

        using var verifyContext = _dbContextFactory.CreateDbContext();
        var count = await verifyContext.Transactions.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_ShouldDisposeDbContextAfterUse()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        await _repository.AddAsync(transaction);

        // Assert - The using statement in repository should dispose the context
        // Verify the transaction was saved by creating a new context
        using var verifyContext = _dbContextFactory.CreateDbContext();
        var savedTransaction = await verifyContext.Transactions.FindAsync(transaction.Id);
        savedTransaction.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithDifferentCurrencies_ShouldSaveCorrectly()
    {
        // Arrange
        var currencies = new[] { "RON", "EUR", "USD", "GBP" };
        var transactions = new List<Transaction>();

        foreach (var currency in currencies)
        {
            transactions.Add(new Transaction(
                100.50m,
                currency,
                ValidIban,
                ValidMerchantId,
                ValidCustomerId,
                ValidTransactionDate));
        }

        // Act
        foreach (var transaction in transactions)
        {
            await _repository.AddAsync(transaction);
        }

        // Assert
        using var verifyContext = _dbContextFactory.CreateDbContext();
        var savedTransactions = await verifyContext.Transactions.ToListAsync();
        savedTransactions.Should().HaveCount(currencies.Length);
        savedTransactions.Select(t => t.Currency).Should().BeEquivalentTo(currencies);
    }

    [Fact]
    public async Task AddAsync_WithNormalizedIban_ShouldPreserveNormalization()
    {
        // Arrange
        var ibanWithSpaces = "RO49 AAAA 1B31 0075 9384 0000";
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ibanWithSpaces,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Iban.Should().Be("RO49AAAA1B31007593840000"); // Normalized (no spaces)
        
        using var verifyContext = _dbContextFactory.CreateDbContext();
        var savedTransaction = await verifyContext.Transactions.FindAsync(result.Id);
        savedTransaction!.Iban.Should().Be("RO49AAAA1B31007593840000");
    }

    public void Dispose()
    {
        // No disposal needed - each context is created fresh
    }

    /// <summary>
    /// In-memory database context factory for testing.
    /// Creates a new context with a shared in-memory database for each test class instance.
    /// </summary>
    private class InMemoryDbContextFactory : IDbContextFactory
    {
        private readonly string _databaseName;

        public InMemoryDbContextFactory()
        {
            _databaseName = Guid.NewGuid().ToString();
        }

        public ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

