namespace StangaNetLib.Core.Pagination;

/// <summary>
/// Wraps a paginated collection of items with metadata.
/// </summary>
public sealed class PagedResult<T>
{
    /// <summary>The items in the current page.</summary>
    public IReadOnlyCollection<T> Items { get; }

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items = items.ToList().AsReadOnly();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>Returns an empty paged result with no items.</summary>
    public static PagedResult<T> Empty(int page = 1, int pageSize = PaginationParams.DefaultPageSize)
        => new([], page, pageSize, 0);

    /// <summary>
    /// Projects each item to a new type, preserving pagination metadata.
    /// Useful to convert domain entities to DTOs without losing page info.
    /// </summary>
    public PagedResult<TTarget> Map<TTarget>(Func<T, TTarget> map)
        => new(Items.Select(map), Page, PageSize, TotalCount);
}
