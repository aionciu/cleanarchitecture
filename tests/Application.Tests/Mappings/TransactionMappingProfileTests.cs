using Application.Commands.CreateTransaction;
using Application.DTOs;
using Application.Mappings;
using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Application.Tests.Mappings;

public class TransactionMappingProfileTests
{
    private readonly IMapper _mapper;

    public TransactionMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TransactionMappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Map_CreateTransactionCommandToTransaction_ShouldMapCorrectly()
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
        var transaction = _mapper.Map<Transaction>(command);

        // Assert
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(command.Amount);
        transaction.Currency.Should().Be(command.Currency);
        transaction.Iban.Should().Be("RO49AAAA1B31007593840000"); // Normalized (no spaces)
        transaction.MerchantId.Should().Be(command.MerchantId);
        transaction.CustomerId.Should().Be(command.CustomerId);
        transaction.TransactionDate.Should().Be(command.TransactionDate);
        transaction.Status.Should().Be(TransactionStatus.Pending); // Always Pending on creation
        transaction.Id.Should().NotBeEmpty();
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Map_CreateTransactionCommandWithIbanSpaces_ShouldNormalizeIban()
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
        var transaction = _mapper.Map<Transaction>(command);

        // Assert
        // The Transaction constructor normalizes the IBAN (removes spaces and dashes, converts to uppercase)
        transaction.Iban.Should().Be("RO49AAAA1B31007593840000"); // Normalized by constructor
        transaction.Iban.Should().NotContain(" "); // No spaces
    }

    [Fact]
    public void Map_TransactionToTransactionDto_ShouldMapCorrectly()
    {
        // Arrange
        var transaction = new Transaction(
            amount: 100.50m,
            currency: "RON",
            iban: "RO49AAAA1B31007593840000",
            merchantId: "MERCHANT-001",
            customerId: "CUSTOMER-001",
            transactionDate: DateTime.UtcNow.AddDays(-1));

        // Act
        var dto = _mapper.Map<TransactionDto>(transaction);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(transaction.Id);
        dto.Amount.Should().Be(transaction.Amount);
        dto.Currency.Should().Be(transaction.Currency);
        dto.Iban.Should().Be(transaction.Iban);
        dto.Status.Should().Be(transaction.Status);
        dto.MerchantId.Should().Be(transaction.MerchantId);
        dto.CustomerId.Should().Be(transaction.CustomerId);
        dto.TransactionDate.Should().Be(transaction.TransactionDate);
        dto.CreatedAt.Should().Be(transaction.CreatedAt);
    }

    [Fact]
    public void Map_TransactionWithDifferentStatus_ShouldMapStatusCorrectly()
    {
        // Arrange
        var transaction = new Transaction(
            amount: 100.50m,
            currency: "RON",
            iban: "RO49AAAA1B31007593840000",
            merchantId: "MERCHANT-001",
            customerId: "CUSTOMER-001",
            transactionDate: DateTime.UtcNow.AddDays(-1));

        transaction.MarkAsCompleted();

        // Act
        var dto = _mapper.Map<TransactionDto>(transaction);

        // Assert
        dto.Status.Should().Be(TransactionStatus.Completed);
    }
}

