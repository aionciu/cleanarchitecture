using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Tests.Data;

public class SqlServerDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_WithValidConnectionString_ShouldReturnDbContext()
    {
        // Arrange
        var connectionString = "Server=test;Database=TestDb;Trusted_Connection=true;";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            })
            .Build();

        var factory = new SqlServerDbContextFactory(configuration);

        // Act
        var context = factory.CreateDbContext();

        // Assert
        context.Should().NotBeNull();
        context.Should().BeOfType<ApplicationDbContext>();
    }

    [Fact]
    public void CreateDbContext_WithNullConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var factory = new SqlServerDbContextFactory(configuration);

        // Act
        var act = () => factory.CreateDbContext();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'DefaultConnection' not found.");
    }

    [Fact]
    public void CreateDbContext_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", string.Empty }
            })
            .Build();

        var factory = new SqlServerDbContextFactory(configuration);

        // Act
        var act = () => factory.CreateDbContext();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Connection string 'DefaultConnection' not found.");
    }

    [Fact]
    public void CreateDbContext_ShouldUseSqlServerProvider()
    {
        // Arrange
        var connectionString = "Server=test;Database=TestDb;Trusted_Connection=true;";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            })
            .Build();

        var factory = new SqlServerDbContextFactory(configuration);

        // Act
        var context = factory.CreateDbContext();

        // Assert
        context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.SqlServer");
    }

    [Fact]
    public void CreateDbContext_EachCall_ShouldReturnNewInstance()
    {
        // Arrange
        var connectionString = "Server=test;Database=TestDb;Trusted_Connection=true;";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            })
            .Build();

        var factory = new SqlServerDbContextFactory(configuration);

        // Act
        var context1 = factory.CreateDbContext();
        var context2 = factory.CreateDbContext();

        // Assert
        context1.Should().NotBeSameAs(context2);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new SqlServerDbContextFactory(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }
}

