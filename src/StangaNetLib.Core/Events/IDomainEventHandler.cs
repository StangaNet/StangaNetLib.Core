namespace StangaNetLib.Core.Events;

/// <summary>
/// Handles a specific domain event type.
/// Register implementations in the DI container; the dispatcher will resolve and invoke them.
/// </summary>
public interface IDomainEventHandler<TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
