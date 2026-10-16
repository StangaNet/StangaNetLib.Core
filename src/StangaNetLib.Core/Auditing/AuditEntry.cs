namespace StangaNetLib.Core.Auditing;

/// <summary>
/// Single entry in an audit trail. Immutable once created.
/// </summary>
public sealed record AuditEntry
{
    /// <summary>Unique identifier of this audit entry. Auto-generated on construction.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Subject of the action (user whose data was processed).</summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Action performed. Use dot-notation for clarity,
    /// e.g. "data.export", "consent.grant", "content.publish".
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>Type of entity involved (e.g. "User", "Order"). Null for generic actions.</summary>
    public string? EntityType { get; init; }

    /// <summary>Identifier of the entity involved. Null for generic actions.</summary>
    public string? EntityId { get; init; }

    /// <summary>Additional context in free-text or JSON format.</summary>
    public string? Details { get; init; }

    /// <summary>IP address of the actor who performed the action.</summary>
    public string? IpAddress { get; init; }

    /// <summary>User-Agent of the actor.</summary>
    public string? UserAgent { get; init; }

    /// <summary>UTC timestamp when the action was performed. Defaults to <see cref="DateTimeOffset.UtcNow"/> at construction.</summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
