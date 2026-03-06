namespace StangaNetLib.Core.Common;

/// <summary>
/// Abstracts database transaction management.
/// Implement in the Infrastructure layer (e.g. wrapping EF Core DbContext transactions).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes to the underlying store.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins an explicit transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
