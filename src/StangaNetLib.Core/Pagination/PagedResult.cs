namespace StangaNetLib.Core.Pagination;

/// <summary>
/// Wraps a paginated collection of items with metadata.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
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

    /// <summary>Total number of pages, derived from <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>True when a previous page exists (i.e. <see cref="Page"/> is greater than 1).</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>True when a next page exists (i.e. <see cref="Page"/> is less than <see cref="TotalPages"/>).</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Initialises a new paged result.</summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items = items.ToList().AsReadOnly();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>Returns an empty paged result with no items.</summary>
    /// <param name="page">The page number to record. Defaults to 1.</param>
    /// <param name="pageSize">The page size to record. Defaults to <see cref="PaginationParams.DefaultPageSize"/>.</param>
    /// <returns>A <see cref="PagedResult{T}"/> with an empty item collection and <see cref="TotalCount"/> of zero.</returns>
    public static PagedResult<T> Empty(int page = 1, int pageSize = PaginationParams.DefaultPageSize)
        => new([], page, pageSize, 0);

    /// <summary>
    /// Projects each item to a new type, preserving pagination metadata.
    /// Useful to convert domain entities to DTOs without losing page info.
    /// </summary>
    /// <typeparam name="TTarget">The projected item type.</typeparam>
    /// <param name="map">Projection applied to each item.</param>
    /// <returns>A new <see cref="PagedResult{TTarget}"/> with the same page metadata and projected items.</returns>
    public PagedResult<TTarget> Map<TTarget>(Func<T, TTarget> map)
        => new(Items.Select(map), Page, PageSize, TotalCount);
}
