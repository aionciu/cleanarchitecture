using Application.Commands.CreateTransaction;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.CreateTransaction;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();

        // Setup AutoMapper
        var configuration = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<Application.Mappings.TransactionMappingProfile>());
        _mapper = configuration.CreateMapper();

        _handler = new CreateTransactionCommandHandler(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnTransactionDto()
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

        var createdTransaction = new Transaction(
            command.Amount,
            command.Currency,
            command.Iban,
            command.MerchantId,
            command.CustomerId,
            command.TransactionDate);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTransaction);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TransactionDto>();
        result.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);
        result.Iban.Should().Be("RO49AAAA1B31007593840000"); // Normalized
        result.MerchantId.Should().Be(command.MerchantId);
        result.CustomerId.Should().Be(command.CustomerId);
        result.TransactionDate.Should().Be(command.TransactionDate);
        result.Status.Should().Be(TransactionStatus.Pending);
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Transaction>(t =>
                t.Amount == command.Amount &&
                t.Currency == command.Currency &&
                t.MerchantId == command.MerchantId &&
                t.CustomerId == command.CustomerId &&
                t.TransactionDate == command.TransactionDate &&
                t.Status == TransactionStatus.Pending),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryWithCorrectTransaction()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 250.75m,
            Currency = "EUR",
            Iban = "GB82WEST12345698765432",
            MerchantId = "MERCHANT-002",
            CustomerId = "CUSTOMER-002",
            TransactionDate = DateTime.UtcNow.AddDays(-2)
        };

        var createdTransaction = new Transaction(
            command.Amount,
            command.Currency,
            command.Iban,
            command.MerchantId,
            command.CustomerId,
            command.TransactionDate);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTransaction);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Transaction>(t =>
                    t.Amount == 250.75m &&
                    t.Currency == "EUR" &&
                    t.Iban == "GB82WEST12345698765432" &&
                    t.MerchantId == "MERCHANT-002" &&
                    t.CustomerId == "CUSTOMER-002" &&
                    t.Status == TransactionStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationTokenToRepository()
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

        var cancellationToken = new CancellationToken();

        var createdTransaction = new Transaction(
            command.Amount,
            command.Currency,
            command.Iban,
            command.MerchantId,
            command.CustomerId,
            command.TransactionDate);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTransaction);

        // Act
        await _handler.HandleAsync(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transaction>(), cancellationToken),
            Times.Once);
    }
}


