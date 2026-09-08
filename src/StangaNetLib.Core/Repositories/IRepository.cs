using StangaNetLib.Core.Common;
using StangaNetLib.Core.Specifications;

namespace StangaNetLib.Core.Repositories;

/// <summary>
/// Generic repository contract for entities keyed by <typeparamref name="TId"/>.
/// Extend this interface in the Domain layer to add domain-specific query methods.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public interface IRepository<T, TId>
    where T : Entity<TId>
    where TId : notnull
{
    /// <summary>Retrieves an entity by its identifier.</summary>
    /// <param name="id">The identifier to look up.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The entity, or <c>null</c> if not found.</returns>
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves all entities. Use with caution on large collections.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All entities in the store.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all entities that satisfy <paramref name="spec"/>.</summary>
    /// <param name="spec">The specification defining the filter, includes, and ordering.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All matching entities, in the order defined by the specification.</returns>
    Task<IEnumerable<T>> FindAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Returns the first entity that satisfies <paramref name="spec"/>, or null.</summary>
    /// <param name="spec">The specification defining the filter criteria.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The first matching entity, or <c>null</c> if none satisfies the specification.</returns>
    Task<T?> FindOneAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Returns the total number of entities that satisfy <paramref name="spec"/>.</summary>
    /// <param name="spec">The specification defining the filter criteria.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The count of matching entities.</returns>
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Adds a new entity to the store (does not persist until SaveChanges).</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Marks an entity as modified (does not persist until SaveChanges).</summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Removes an entity from the store (does not persist until SaveChanges).</summary>
    /// <param name="entity">The entity to remove.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Returns true if an entity with the given id exists.</summary>
    /// <param name="id">The identifier to check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> when an entity with <paramref name="id"/> exists; <c>false</c> otherwise.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>Returns true if at least one entity satisfies <paramref name="spec"/>.</summary>
    /// <param name="spec">The specification defining the filter criteria.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> when at least one entity satisfies the specification; <c>false</c> otherwise.</returns>
    Task<bool> ExistsBySpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Adds multiple entities in bulk (does not persist until SaveChanges).</summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>Removes multiple entities in bulk (does not persist until SaveChanges).</summary>
    /// <param name="entities">The entities to remove.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience alias for repositories whose entities are keyed by <see cref="Guid"/>.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IRepository<T> : IRepository<T, Guid>
    where T : Entity { }
