using StangaNetLib.Core.Events;

namespace StangaNetLib.Core.Common;

/// <summary>
/// Base class for domain entities identified by a key of type <typeparamref name="TId"/>.
/// Supports domain events and structural equality by Id.
/// </summary>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>Entity identifier.</summary>
    public TId Id { get; protected set; }

    /// <summary>UTC timestamp of creation.</summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>UTC timestamp of last update. Null until first update.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Raises a domain event to be dispatched after the operation completes.</summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>Clears all pending domain events (called by the dispatcher after dispatching).</summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();

    /// <summary>Sets UpdatedAt to the current UTC time. Call at the end of mutating methods.</summary>
    protected void MarkUpdated()
        => UpdatedAt = DateTime.UtcNow;

    // --- Structural equality ---

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
        => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left is null && right is null || (left is not null && left.Equals(right));

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
    protected Entity(Guid id) : base(id) { }
}
