using Application.Commands.CreateTransaction;
using Application.Validators;
using FluentAssertions;
using FluentValidation;

namespace Application.Tests.Validators;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator;

    public CreateTransactionCommandValidatorTests()
    {
        _validator = new CreateTransactionCommandValidator();
    }

    #region Valid Command Tests

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("RON")]
    public void Validate_WithValidCurrency_ShouldPass(string currency)
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = currency,
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithIbanContainingSpaces_ShouldPass()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO 49 AAAA 1B31 0075 9384 0000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Amount Validation Tests

    [Fact]
    public void Validate_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 0,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount" && e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = -100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_WithAmountExceedingMaximum_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 1000000.00m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount" && e.ErrorMessage.Contains("999999.99"));
    }

    #endregion

    #region Currency Validation Tests

    [Fact]
    public void Validate_WithEmptyCurrency_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = string.Empty,
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_WithCurrencyTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "EURO",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Validate_WithLowercaseCurrency_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "ron",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    #endregion

    #region IBAN Validation Tests

    [Fact]
    public void Validate_WithEmptyIban_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = string.Empty,
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Iban");
    }

    [Fact]
    public void Validate_WithIbanTooShort_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO123",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Iban");
    }

    [Fact]
    public void Validate_WithIbanTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000" + new string('A', 20), // Too long
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Iban");
    }

    [Fact]
    public void Validate_WithInvalidIbanFormat_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "123456789012345", // Doesn't start with 2 letters
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Iban");
    }

    #endregion

    #region MerchantId Validation Tests

    [Fact]
    public void Validate_WithEmptyMerchantId_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = string.Empty,
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MerchantId");
    }

    [Fact]
    public void Validate_WithMerchantIdTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = new string('A', 101), // 101 characters
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MerchantId");
    }

    [Fact]
    public void Validate_WithMerchantIdInvalidCharacters_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT@001", // Invalid character @
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MerchantId");
    }

    #endregion

    #region CustomerId Validation Tests

    [Fact]
    public void Validate_WithEmptyCustomerId_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = string.Empty,
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_WithCustomerIdTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = new string('A', 101), // 101 characters
            TransactionDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    #endregion

    #region TransactionDate Validation Tests

    [Fact]
    public void Validate_WithFutureTransactionDate_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TransactionDate" && e.ErrorMessage.Contains("future"));
    }

    [Fact]
    public void Validate_WithTransactionDateOlderThanOneYear_ShouldFail()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100.50m,
            Currency = "RON",
            Iban = "RO49AAAA1B31007593840000",
            MerchantId = "MERCHANT-001",
            CustomerId = "CUSTOMER-001",
            TransactionDate = DateTime.UtcNow.AddYears(-2)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TransactionDate" && e.ErrorMessage.Contains("1 year"));
    }

    #endregion
}


