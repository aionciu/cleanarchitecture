namespace Application.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public NotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a resource name and identifier.
    /// </summary>
    /// <param name="resourceName">The name of the resource (e.g., "Transaction").</param>
    /// <param name="identifier">The identifier that was not found.</param>
    public NotFoundException(string resourceName, object identifier)
        : base($"{resourceName} with identifier '{identifier}' was not found.")
    {
        ResourceName = resourceName;
        Identifier = identifier?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the name of the resource that was not found.
    /// </summary>
    public string? ResourceName { get; }

    /// <summary>
    /// Gets the identifier that was not found.
    /// </summary>
    public string? Identifier { get; }
}

