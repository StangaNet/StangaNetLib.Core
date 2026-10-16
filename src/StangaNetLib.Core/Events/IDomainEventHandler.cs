namespace StangaNetLib.Core.Events;

/// <summary>
/// Handles a specific domain event type.
/// Register implementations in the DI container; the dispatcher will resolve and invoke them.
/// </summary>
/// <typeparam name="TEvent">The domain event type this handler processes.</typeparam>
public interface IDomainEventHandler<TEvent> where TEvent : DomainEvent
{
    /// <summary>Handles the domain event.</summary>
    /// <param name="domainEvent">The event to handle.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
