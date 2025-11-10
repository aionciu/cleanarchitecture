using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Transaction entity.
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    /// <summary>
    /// Configures the Transaction entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Table name
        builder.ToTable("Transactions");

        // Primary key
        builder.HasKey(t => t.Id);

        // Property configurations
        builder.Property(t => t.Id)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .IsFixedLength();

        builder.Property(t => t.Iban)
            .HasMaxLength(34)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.MerchantId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.CustomerId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.TransactionDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        // RowVersion for optimistic concurrency
        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes for query performance
        builder.HasIndex(t => t.TransactionDate)
            .HasDatabaseName("IX_Transactions_TransactionDate");

        builder.HasIndex(t => t.MerchantId)
            .HasDatabaseName("IX_Transactions_MerchantId");

        builder.HasIndex(t => t.CustomerId)
            .HasDatabaseName("IX_Transactions_CustomerId");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Transactions_Status");

        // Composite index for common query patterns
        builder.HasIndex(t => new { t.MerchantId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_MerchantId_TransactionDate");
    }
}

