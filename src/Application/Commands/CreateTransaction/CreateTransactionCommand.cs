using Application.DTOs;
using Application.Mediator;

namespace Application.Commands.CreateTransaction;

/// <summary>
/// Command to create a new transaction.
/// </summary>
public class CreateTransactionCommand : IRequest<TransactionDto>
{
    /// <summary>
    /// Gets or sets the transaction amount. Must be greater than 0.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217 format, e.g., "RON", "EUR", "USD").
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IBAN (International Bank Account Number).
    /// </summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant identifier.
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction date. Cannot be in the future.
    /// </summary>
    public DateTime TransactionDate { get; set; }
}


