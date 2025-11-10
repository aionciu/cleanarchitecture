using Application.Commands.CreateTransaction;
using FluentValidation;

namespace Application.Validators;

/// <summary>
/// Validator for CreateTransactionCommand.
/// </summary>
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTransactionCommandValidator"/> class.
    /// </summary>
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Amount must not exceed 999999.99.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .MaximumLength(3)
            .WithMessage("Currency must be a valid ISO 4217 code (max 3 characters).")
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Currency must be a valid ISO 4217 code (3 uppercase letters).");

        RuleFor(x => x.Iban)
            .NotEmpty()
            .WithMessage("IBAN is required.")
            .Must(BeValidIbanFormat)
            .WithMessage("IBAN must be between 15 and 34 characters and start with 2 letters followed by 2 digits, then alphanumeric characters.");

        RuleFor(x => x.MerchantId)
            .NotEmpty()
            .WithMessage("MerchantId is required.")
            .MaximumLength(100)
            .WithMessage("MerchantId must not exceed 100 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("MerchantId must contain only alphanumeric characters, dashes, and underscores.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required.")
            .MaximumLength(100)
            .WithMessage("CustomerId must not exceed 100 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("CustomerId must contain only alphanumeric characters, dashes, and underscores.");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("TransactionDate is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("TransactionDate cannot be in the future.")
            .Must(BeWithinOneYear)
            .WithMessage("TransactionDate cannot be older than 1 year.");
    }

    private static bool BeValidIbanFormat(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return false;
        }

        // Normalize IBAN: remove spaces and dashes, convert to uppercase
        var normalizedIban = iban.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        // Check length: 15-34 characters
        if (normalizedIban.Length < 15 || normalizedIban.Length > 34)
        {
            return false;
        }

        // Check format: starts with 2 letters, then 2 digits, then alphanumeric
        return System.Text.RegularExpressions.Regex.IsMatch(normalizedIban, @"^[A-Z]{2}\d{2}[A-Z0-9]+$");
    }

    private static bool BeWithinOneYear(DateTime transactionDate)
    {
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        return transactionDate >= oneYearAgo;
    }
}


