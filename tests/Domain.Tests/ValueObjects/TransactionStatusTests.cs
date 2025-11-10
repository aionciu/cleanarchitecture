using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class TransactionStatusTests
{
    [Fact]
    public void TransactionStatus_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act & Assert
        Enum.GetValues<TransactionStatus>().Should().HaveCount(5);
        ((int)TransactionStatus.Pending).Should().Be(0);
        ((int)TransactionStatus.Processing).Should().Be(1);
        ((int)TransactionStatus.Completed).Should().Be(2);
        ((int)TransactionStatus.Failed).Should().Be(3);
        ((int)TransactionStatus.Cancelled).Should().Be(4);
    }

    [Theory]
    [InlineData(TransactionStatus.Pending)]
    [InlineData(TransactionStatus.Processing)]
    [InlineData(TransactionStatus.Completed)]
    [InlineData(TransactionStatus.Failed)]
    [InlineData(TransactionStatus.Cancelled)]
    public void TransactionStatus_ShouldBeValidEnumValue(TransactionStatus status)
    {
        // Arrange & Act & Assert
        Enum.IsDefined(typeof(TransactionStatus), status).Should().BeTrue();
    }
}

