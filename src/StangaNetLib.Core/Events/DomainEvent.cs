namespace StangaNetLib.Core.Events;

/// <summary>
/// Base class for all domain events. Each event gets a unique ID and timestamp on creation.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the event occurred.</summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
