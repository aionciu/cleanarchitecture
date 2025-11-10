using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

/// <summary>
/// SQL Server implementation of the database context factory.
/// </summary>
public class SqlServerDbContextFactory : IDbContextFactory
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDbContextFactory"/> class.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    public SqlServerDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Creates a new instance of the application database context.
    /// </summary>
    /// <returns>A new instance of <see cref="ApplicationDbContext"/>.</returns>
    public ApplicationDbContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

