namespace StangaNetLib.Core.Events;

/// <summary>
/// Dispatches domain events to their registered handlers.
/// Implement this interface in the Infrastructure layer.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>Dispatches a single typed event to all registered handlers.</summary>
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;

    /// <summary>Dispatches a collection of events (e.g. all events raised by an aggregate).</summary>
    Task DispatchManyAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
