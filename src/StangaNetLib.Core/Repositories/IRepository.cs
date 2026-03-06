using StangaNetLib.Core.Common;

namespace StangaNetLib.Core.Repositories;

/// <summary>
/// Generic repository contract for entities keyed by <typeparamref name="TId"/>.
/// Extend this interface in the Domain layer to add domain-specific query methods.
/// </summary>
public interface IRepository<T, TId>
    where T : Entity<TId>
    where TId : notnull
{
    /// <summary>Retrieves an entity by its identifier. Returns null if not found.</summary>
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all entities. Use with caution on large collections.</summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity to the store (does not persist until SaveChanges).</summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Marks an entity as modified (does not persist until SaveChanges).</summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Removes an entity from the store (does not persist until SaveChanges).</summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Returns true if an entity with the given id exists.</summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience alias for repositories whose entities are keyed by <see cref="Guid"/>.
/// </summary>
public interface IRepository<T> : IRepository<T, Guid>
    where T : Entity { }
