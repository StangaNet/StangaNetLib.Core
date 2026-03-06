namespace StangaNetLib.Core.Pagination;

/// <summary>
/// Input parameters for paginated queries.
/// PageSize is automatically capped at <see cref="MaxPageSize"/>.
/// </summary>
public class PaginationParams
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number. Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page. Capped at <see cref="MaxPageSize"/>.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>Number of items to skip (derived from Page and PageSize).</summary>
    public int Skip => (Page - 1) * PageSize;
}
