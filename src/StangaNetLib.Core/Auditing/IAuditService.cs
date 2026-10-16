namespace StangaNetLib.Core.Auditing;

/// <summary>
/// Immutable audit trail for recording significant application events.
/// The consuming project provides an EF Core or other persistence implementation.
/// </summary>
public interface IAuditService
{
    /// <summary>Appends a new entry to the audit log.</summary>
    /// <param name="userId">Identifier of the user whose data or action is being recorded.</param>
    /// <param name="action">Dot-notation action name (e.g. "data.export", "consent.grant", "content.publish").</param>
    /// <param name="entityType">Type of entity involved (e.g. "User", "Order"). Null for generic actions.</param>
    /// <param name="entityId">String representation of the entity's identifier. Null for generic actions.</param>
    /// <param name="details">Additional context in free-text or JSON format.</param>
    /// <param name="ipAddress">IP address of the actor performing the action.</param>
    /// <param name="userAgent">User-Agent of the actor's HTTP client.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task LogAsync(
        string            userId,
        string            action,
        string?           entityType = null,
        string?           entityId   = null,
        string?           details    = null,
        string?           ipAddress  = null,
        string?           userAgent  = null,
        CancellationToken ct         = default);

    /// <summary>
    /// Retrieves audit log entries for the specified user.
    /// Supports optional filters by date range and action prefix.
    /// Results are ordered by <see cref="AuditEntry.OccurredAt"/> descending.
    /// </summary>
    /// <param name="userId">Identifier of the user whose audit entries to retrieve.</param>
    /// <param name="from">Inclusive lower bound on <see cref="AuditEntry.OccurredAt"/>. Null means no lower bound.</param>
    /// <param name="to">Inclusive upper bound on <see cref="AuditEntry.OccurredAt"/>. Null means no upper bound.</param>
    /// <param name="action">Prefix filter on <see cref="AuditEntry.Action"/> (e.g. "consent" matches "consent.grant"). Null means no filter.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>Matching audit entries ordered by <see cref="AuditEntry.OccurredAt"/> descending.</returns>
    Task<IReadOnlyList<AuditEntry>> GetLogAsync(
        string            userId,
        DateTimeOffset?   from   = null,
        DateTimeOffset?   to     = null,
        string?           action = null,
        CancellationToken ct     = default);

    /// <summary>
    /// Deletes audit entries older than <paramref name="retentionDays"/>.
    /// Intended for scheduled data-retention cleanup jobs.
    /// </summary>
    /// <param name="retentionDays">Entries with <see cref="AuditEntry.OccurredAt"/> older than this many days are deleted.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>The number of deleted entries.</returns>
    Task<int> DeleteExpiredAsync(int retentionDays, CancellationToken ct = default);
}
