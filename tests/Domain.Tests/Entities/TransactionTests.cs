using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Entities;

public class TransactionTests
{
    private static readonly string ValidIban = "RO49AAAA1B31007593840000";
    private static readonly string ValidCurrency = "RON";
    private static readonly string ValidMerchantId = "MERCHANT-001";
    private static readonly string ValidCustomerId = "CUSTOMER-001";
    private static readonly decimal ValidAmount = 100.50m;
    private static readonly DateTime ValidTransactionDate = DateTime.UtcNow.AddDays(-1);

    #region Valid Creation Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTransaction()
    {
        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeEmpty();
        transaction.Amount.Should().Be(ValidAmount);
        transaction.Currency.Should().Be(ValidCurrency);
        transaction.Iban.Should().Be(ValidIban);
        transaction.MerchantId.Should().Be(ValidMerchantId);
        transaction.CustomerId.Should().Be(ValidCustomerId);
        transaction.TransactionDate.Should().Be(ValidTransactionDate);
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldGenerateUniqueIds()
    {
        // Act
        var transaction1 = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var transaction2 = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction1.Id.Should().NotBe(transaction2.Id);
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("RON")]
    public void Constructor_WithDifferentCurrencies_ShouldAcceptValidCurrency(string currency)
    {
        // Act
        var transaction = new Transaction(
            ValidAmount,
            currency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Currency.Should().Be(currency.ToUpperInvariant());
    }

    [Fact]
    public void Constructor_WithIbanContainingSpaces_ShouldNormalizeIban()
    {
        // Arrange
        var ibanWithSpaces = "RO 49 AAAA 1B31 0075 9384 0000";

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ibanWithSpaces,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Iban.Should().Be("RO49AAAA1B31007593840000");
    }

    [Fact]
    public void Constructor_WithIbanContainingDashes_ShouldNormalizeIban()
    {
        // Arrange
        var ibanWithDashes = "RO-49-AAAA-1B31-0075-9384-0000";

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ibanWithDashes,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Iban.Should().Be("RO49AAAA1B31007593840000");
    }

    [Fact]
    public void Constructor_WithLowercaseIban_ShouldConvertToUppercase()
    {
        // Arrange
        var lowercaseIban = "ro49aaaa1b31007593840000";

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            lowercaseIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Iban.Should().Be("RO49AAAA1B31007593840000");
    }

    [Fact]
    public void Constructor_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange
        var lowercaseCurrency = "ron";

        // Act
        var transaction = new Transaction(
            ValidAmount,
            lowercaseCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Currency.Should().Be("RON");
    }

    #endregion

    #region Amount Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Constructor_WithInvalidAmount_ShouldThrowArgumentException(decimal amount)
    {
        // Act
        var act = () => new Transaction(
            amount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount")
            .WithMessage("Amount must be greater than 0.*");
    }

    [Fact]
    public void Constructor_WithPositiveAmount_ShouldAccept()
    {
        // Act
        var transaction = new Transaction(
            0.01m,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Constructor_WithMinValueAmount_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Transaction(
            decimal.MinValue,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount")
            .WithMessage("Amount must be greater than 0.*");
    }

    #endregion

    #region Currency Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Act
        var act = () => new Transaction(
            ValidAmount,
            currency!,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency")
            .WithMessage("Currency cannot be null or empty.*");
    }

    [Fact]
    public void Constructor_WithCurrencyLongerThan3Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidCurrency = "EURO";

        // Act
        var act = () => new Transaction(
            ValidAmount,
            invalidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency")
            .WithMessage("Currency must be a valid ISO 4217 code (max 3 characters).*");
    }

    #endregion

    #region IBAN Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyIban_ShouldThrowArgumentException(string? iban)
    {
        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            iban!,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("iban")
            .WithMessage("IBAN cannot be null or empty.*");
    }

    [Fact]
    public void Constructor_WithIbanShorterThan15Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var shortIban = "RO123456789012"; // 14 characters

        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            shortIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("iban")
            .WithMessage("IBAN must be between 15 and 34 characters.*");
    }

    [Fact]
    public void Constructor_WithIbanLongerThan34Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longIban = "RO" + new string('A', 33); // 35 characters

        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            longIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("iban")
            .WithMessage("IBAN must be between 15 and 34 characters.*");
    }

    [Theory]
    [InlineData("123456789012345")] // No letters at start
    [InlineData("R12345678901234")] // Only 1 letter
    [InlineData("12RO123456789012")] // Numbers before letters
    public void Constructor_WithInvalidIbanFormat_ShouldThrowArgumentException(string invalidIban)
    {
        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            invalidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("iban")
            .WithMessage("IBAN must start with 2 letters followed by 2 digits, then alphanumeric characters.*");
    }


    [Fact]
    public void Constructor_WithValidIbanFormat_ShouldAccept()
    {
        // Arrange
        var validIban = "GB82WEST12345698765432"; // Valid UK IBAN format

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            validIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Iban.Should().Be("GB82WEST12345698765432");
    }

    #endregion

    #region MerchantId Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyMerchantId_ShouldThrowArgumentException(string? merchantId)
    {
        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            merchantId!,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("merchantId")
            .WithMessage("MerchantId cannot be null or empty.*");
    }

    #endregion

    #region CustomerId Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullOrEmptyCustomerId_ShouldThrowArgumentException(string? customerId)
    {
        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            customerId!,
            ValidTransactionDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("customerId")
            .WithMessage("CustomerId cannot be null or empty.*");
    }

    #endregion

    #region TransactionDate Validation Tests

    [Fact]
    public void Constructor_WithFutureTransactionDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            futureDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("transactionDate")
            .WithMessage("TransactionDate cannot be in the future.*");
    }

    [Fact]
    public void Constructor_WithPastTransactionDate_ShouldAccept()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            pastDate);

        // Assert
        transaction.TransactionDate.Should().Be(pastDate);
    }

    [Fact]
    public void Constructor_WithCurrentTransactionDate_ShouldAccept()
    {
        // Arrange
        var currentDate = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            currentDate);

        // Assert
        transaction.TransactionDate.Should().Be(currentDate);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Constructor_ShouldAlwaysSetStatusToPending()
    {
        // Act
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Pending);
    }

    #endregion

    #region Domain Methods Tests

    [Fact]
    public void MarkAsProcessing_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var originalUpdatedAt = transaction.UpdatedAt;
        Thread.Sleep(10); // Small delay to ensure time difference

        // Act
        transaction.MarkAsProcessing();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Processing);
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsCompleted_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var originalUpdatedAt = transaction.UpdatedAt;
        Thread.Sleep(10); // Small delay to ensure time difference

        // Act
        transaction.MarkAsCompleted();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Completed);
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var originalUpdatedAt = transaction.UpdatedAt;
        Thread.Sleep(10); // Small delay to ensure time difference

        // Act
        transaction.MarkAsFailed();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Failed);
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsFailed_WithReason_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var originalUpdatedAt = transaction.UpdatedAt;
        var reason = "Insufficient funds";
        Thread.Sleep(10); // Small delay to ensure time difference

        // Act
        transaction.MarkAsFailed(reason);

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Failed);
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void MarkAsCancelled_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        var originalUpdatedAt = transaction.UpdatedAt;
        Thread.Sleep(10); // Small delay to ensure time difference

        // Act
        transaction.MarkAsCancelled();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void DomainMethods_ShouldAllowStatusTransitions()
    {
        // Arrange
        var transaction = new Transaction(
            ValidAmount,
            ValidCurrency,
            ValidIban,
            ValidMerchantId,
            ValidCustomerId,
            ValidTransactionDate);

        // Act & Assert - Test status transitions
        transaction.Status.Should().Be(TransactionStatus.Pending);

        transaction.MarkAsProcessing();
        transaction.Status.Should().Be(TransactionStatus.Processing);

        transaction.MarkAsCompleted();
        transaction.Status.Should().Be(TransactionStatus.Completed);
    }

    #endregion
}

