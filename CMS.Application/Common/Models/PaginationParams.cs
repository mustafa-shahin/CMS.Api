namespace CMS.Application.Common.Models;

/// <summary>
/// Parameters for paginated queries with optional search and sorting.
/// </summary>
public record PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// The page number to retrieve (1-based).
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// The number of items per page (max 100).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? DefaultPageSize : value);
    }

    /// <summary>
    /// Optional search term for filtering results.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// The property to sort by.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; init; }
}