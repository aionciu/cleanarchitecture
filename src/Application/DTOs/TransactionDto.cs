using Domain.ValueObjects;

namespace Application.DTOs;

/// <summary>
/// Data transfer object for a transaction.
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the transaction.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217 format).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IBAN (International Bank Account Number).
    /// </summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the transaction.
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the merchant identifier.
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction date.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}


