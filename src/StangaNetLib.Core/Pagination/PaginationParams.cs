namespace StangaNetLib.Core.Pagination;

/// <summary>
/// Input parameters for paginated queries.
/// PageSize is automatically capped at <see cref="MaxPageSize"/>.
/// </summary>
public class PaginationParams
{
    /// <summary>Default number of items per page when no explicit page size is specified. Value: 20.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Maximum number of items per page that will be honoured. Value: 100.</summary>
    public const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number. Values below 1 are clamped to 1.</summary>
    public int Page
    {
        get => _page;
        set => _page = Math.Max(1, value);
    }

    /// <summary>Number of items per page. Values are clamped to [1, <see cref="MaxPageSize"/>].</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>Number of items to skip, derived from <see cref="Page"/> and <see cref="PageSize"/>.</summary>
    public int Skip => (Page - 1) * PageSize;
}
