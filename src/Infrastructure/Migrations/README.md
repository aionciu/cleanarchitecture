# Database Migrations

## Initial Migration

The `InitialCreate` migration is a **one-time migration** that should be applied only once at the beginning of the project to set up the initial database schema.

### What it creates:

- **Transactions table** with all required columns:
  - `Id` (Guid, Primary Key)
  - `Amount` (decimal(18,2))
  - `Currency` (nchar(3), fixed length)
  - `Iban` (nvarchar(34))
  - `Status` (int - TransactionStatus enum)
  - `MerchantId` (nvarchar(100))
  - `CustomerId` (nvarchar(100))
  - `TransactionDate` (datetime2)
  - `CreatedAt` (datetime2)
  - `UpdatedAt` (datetime2)
  - `RowVersion` (rowversion - for optimistic concurrency)

- **Indexes**:
  - `IX_Transactions_CustomerId`
  - `IX_Transactions_MerchantId`
  - `IX_Transactions_Status`
  - `IX_Transactions_TransactionDate`
  - `IX_Transactions_MerchantId_TransactionDate` (composite)

### Applying the Migration

To apply this migration to your database:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Important Notes

⚠️ **This migration should only be run once** on a fresh database. If you need to modify the schema later, create a new migration instead of modifying this one.

### Future Migrations

For any schema changes after the initial setup, create new migrations using:

```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Api
```

