namespace StangaNetLib.Core.Common;

/// <summary>
/// Base class for domain entities identified by a key of type <typeparamref name="TId"/>.
/// Provides Id, timestamps, and structural equality. For aggregate roots that raise domain
/// events, derive from <see cref="AggregateRoot{TId}"/> instead.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>Entity identifier.</summary>
    public TId Id { get; protected set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTimeOffset CreatedAt { get; protected set; }

    /// <summary>UTC timestamp of last update. Null until the first mutation.</summary>
    public DateTimeOffset? UpdatedAt { get; protected set; }

    /// <summary>Initialises the entity with the given identifier and sets <see cref="CreatedAt"/> to the current UTC time.</summary>
    /// <param name="id">The entity's identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Sets <see cref="UpdatedAt"/> to the current UTC time. Call at the end of mutating methods.</summary>
    protected void MarkUpdated()
        => UpdatedAt = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => EqualityComparer<TId>.Default.GetHashCode(Id);

    /// <summary>Returns true when both entities are of the same type and share the same identifier, or both are null.</summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left is null && right is null || (left is not null && left.Equals(right));

    /// <summary>Returns true when the two entities differ by type or identifier.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}

/// <summary>
/// Convenience base class for entities keyed by <see cref="Guid"/> (the most common case).
/// Automatically generates a new Guid on construction.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>Creates a new entity with a randomly generated Guid.</summary>
    protected Entity() : base(Guid.NewGuid()) { }

    /// <summary>Creates an entity with a specific Guid (e.g. when rehydrating from persistence).</summary>
    /// <param name="id">The entity's identifier.</param>
    protected Entity(Guid id) : base(id) { }
}
