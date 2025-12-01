using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Common.Models;

/// <summary>
/// Generic paginated list for returning paged results.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public sealed class PaginatedList<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// The page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    /// <summary>
    /// Creates a paginated list from an IQueryable source.
    /// </summary>
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Creates a paginated list from an enumerable source (for in-memory paging).
    /// </summary>
    public static PaginatedList<T> Create(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var items = source.ToList();
        var count = items.Count;
        var pagedItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<T>(pagedItems, count, pageNumber, pageSize);
    }
}