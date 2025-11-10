using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Represents a payment transaction entity.
/// </summary>
public class Transaction
{
    // Private parameterless constructor for EF Core
    private Transaction()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transaction"/> class.
    /// </summary>
    /// <param name="amount">The transaction amount. Must be greater than 0.</param>
    /// <param name="currency">The currency code (ISO 4217 format, e.g., "RON", "EUR", "USD").</param>
    /// <param name="iban">The IBAN (International Bank Account Number).</param>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="transactionDate">The transaction date. Cannot be in the future.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public Transaction(
        decimal amount,
        string currency,
        string iban,
        string merchantId,
        string customerId,
        DateTime transactionDate)
    {
        // Validate amount
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));
        }

        // Validate currency
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));
        }

        if (currency.Length > 3)
        {
            throw new ArgumentException("Currency must be a valid ISO 4217 code (max 3 characters).", nameof(currency));
        }

        // Validate IBAN
        if (string.IsNullOrWhiteSpace(iban))
        {
            throw new ArgumentException("IBAN cannot be null or empty.", nameof(iban));
        }

        // Basic IBAN format validation: 15-34 characters, alphanumeric, starts with 2 letters then 2 digits
        var normalizedIban = iban.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        if (normalizedIban.Length < 15 || normalizedIban.Length > 34)
        {
            throw new ArgumentException("IBAN must be between 15 and 34 characters.", nameof(iban));
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizedIban, @"^[A-Z]{2}\d{2}[A-Z0-9]+$"))
        {
            throw new ArgumentException("IBAN must start with 2 letters followed by 2 digits, then alphanumeric characters.", nameof(iban));
        }

        // Validate merchant ID
        if (string.IsNullOrWhiteSpace(merchantId))
        {
            throw new ArgumentException("MerchantId cannot be null or empty.", nameof(merchantId));
        }

        // Validate customer ID
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("CustomerId cannot be null or empty.", nameof(customerId));
        }

        // Validate transaction date
        if (transactionDate > DateTime.UtcNow)
        {
            throw new ArgumentException("TransactionDate cannot be in the future.", nameof(transactionDate));
        }

        // Set properties
        Id = Guid.NewGuid();
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        Iban = normalizedIban;
        MerchantId = merchantId;
        CustomerId = customerId;
        TransactionDate = transactionDate;
        Status = TransactionStatus.Pending; // Always Pending on creation
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the currency code (ISO 4217 format).
    /// </summary>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the IBAN (International Bank Account Number).
    /// </summary>
    public string Iban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current status of the transaction.
    /// </summary>
    public TransactionStatus Status { get; private set; }

    /// <summary>
    /// Gets the merchant identifier.
    /// </summary>
    public string MerchantId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the customer identifier.
    /// </summary>
    public string CustomerId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the transaction date.
    /// </summary>
    public DateTime TransactionDate { get; private set; }

    /// <summary>
    /// Gets the creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the row version for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; private set; }

    /// <summary>
    /// Marks the transaction as processing.
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = TransactionStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the transaction as completed.
    /// </summary>
    public void MarkAsCompleted()
    {
        Status = TransactionStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the transaction as failed.
    /// </summary>
    /// <param name="reason">Optional reason for the failure (for future logging/auditing).</param>
    public void MarkAsFailed(string? reason = null)
    {
        Status = TransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
        // Note: Reason parameter reserved for future use (logging/auditing)
    }

    /// <summary>
    /// Marks the transaction as cancelled.
    /// </summary>
    public void MarkAsCancelled()
    {
        Status = TransactionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}


