namespace Infrastructure.Data;

/// <summary>
/// Factory interface for creating database contexts.
/// </summary>
public interface IDbContextFactory
{
    /// <summary>
    /// Creates a new instance of the application database context.
    /// </summary>
    /// <returns>A new instance of <see cref="ApplicationDbContext"/>.</returns>
    ApplicationDbContext CreateDbContext();
}

