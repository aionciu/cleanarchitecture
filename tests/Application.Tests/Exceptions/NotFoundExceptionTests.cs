using Application.Exceptions;

namespace Application.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.ResourceName);
        Assert.Null(exception.Identifier);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Resource not found";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new NotFoundException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
        Assert.Null(exception.ResourceName);
        Assert.Null(exception.Identifier);
    }

    [Fact]
    public void Constructor_WithResourceNameAndIdentifier_ShouldSetProperties()
    {
        // Arrange
        var resourceName = "Transaction";
        var identifier = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, identifier);

        // Assert
        Assert.Contains(resourceName, exception.Message);
        Assert.Contains(identifier.ToString(), exception.Message);
        Assert.Equal(resourceName, exception.ResourceName);
        Assert.Equal(identifier.ToString(), exception.Identifier);
    }

    [Fact]
    public void Constructor_WithResourceNameAndStringIdentifier_ShouldSetProperties()
    {
        // Arrange
        var resourceName = "Transaction";
        var identifier = "TXN-123";

        // Act
        var exception = new NotFoundException(resourceName, identifier);

        // Assert
        Assert.Contains(resourceName, exception.Message);
        Assert.Contains(identifier, exception.Message);
        Assert.Equal(resourceName, exception.ResourceName);
        Assert.Equal(identifier, exception.Identifier);
    }

    [Fact]
    public void Constructor_WithResourceNameAndNullIdentifier_ShouldHandleNull()
    {
        // Arrange
        var resourceName = "Transaction";
        object? identifier = null;

        // Act
        var exception = new NotFoundException(resourceName, identifier!);

        // Assert
        Assert.Contains(resourceName, exception.Message);
        Assert.Equal(resourceName, exception.ResourceName);
        Assert.Equal(string.Empty, exception.Identifier);
    }

    [Fact]
    public void Constructor_WithResourceNameAndIdentifier_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var resourceName = "User";
        var identifier = 42;

        // Act
        var exception = new NotFoundException(resourceName, identifier);

        // Assert
        Assert.Equal($"{resourceName} with identifier '{identifier}' was not found.", exception.Message);
    }
}
