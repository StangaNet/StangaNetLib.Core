using StangaNetLib.Core.Events;

namespace StangaNetLib.Core.Common;

/// <summary>
/// Base class for DDD aggregate roots. Extends <see cref="Entity{TId}"/> with domain event
/// collection. Only aggregate roots should raise and own domain events; child entities should not.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this operation, to be dispatched after persistence.</summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Initialises a new aggregate root with the given identifier.</summary>
    /// <param name="id">The aggregate root's identifier.</param>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Raises a domain event to be dispatched after the operation completes.</summary>
    /// <param name="domainEvent">The event to enqueue.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>Removes a specific domain event that was previously raised (e.g. to cancel a side effect before persistence).</summary>
    /// <param name="domainEvent">The event to remove. No-op if the event is not in the pending list.</param>
    protected void RemoveDomainEvent(DomainEvent domainEvent)
        => _domainEvents.Remove(domainEvent);

    /// <summary>Clears all pending domain events (called by the dispatcher after dispatching).</summary>
    public void ClearDomainEvents()
        => _domainEvents.Clear();
}

/// <summary>
/// Convenience base class for aggregate roots keyed by <see cref="Guid"/> (the most common case).
/// Automatically generates a new Guid on construction.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    /// <summary>Creates a new aggregate root with a randomly generated Guid.</summary>
    protected AggregateRoot() : base(Guid.NewGuid()) { }

    /// <summary>Creates an aggregate root with a specific Guid (e.g. when rehydrating from persistence).</summary>
    protected AggregateRoot(Guid id) : base(id) { }
}
