namespace Domain.ValueObjects;

/// <summary>
/// Represents the status of a transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction created but not processed.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction is being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Transaction successfully completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Transaction failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Transaction was cancelled.
    /// </summary>
    Cancelled = 4
}


