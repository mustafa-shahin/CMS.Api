namespace CMS.Application.Common.Models.Search;

/// <summary>
/// Generic search result with pagination and relevance scoring
/// </summary>
public sealed record SearchResult<T> where T : class
{
    /// <summary>
    /// Search result items
    /// </summary>
    public List<SearchResultItem<T>> Items { get; set; } = new();

    /// <summary>
    /// Total count of items matching search criteria (before pagination)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Search execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Applied filters (for debugging/transparency)
    /// </summary>
    public List<FilterCriteria>? AppliedFilters { get; set; }

    /// <summary>
    /// Applied sorts (for debugging/transparency)
    /// </summary>
    public List<SortCriteria>? AppliedSorts { get; set; }

    /// <summary>
    /// Search term that was used
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Facets/aggregations (for faceted search)
    /// </summary>
    public Dictionary<string, List<FacetValue>>? Facets { get; set; }
}

/// <summary>
/// Single search result item with relevance score
/// </summary>
public sealed record SearchResultItem<T> where T : class
{
    /// <summary>
    /// The actual data item
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// Relevance score (0.0 - 1.0) for full-text search
    /// </summary>
    public double? RelevanceScore { get; set; }

    /// <summary>
    /// Highlighted fields (field name -> highlighted content with <mark> tags)
    /// </summary>
    public Dictionary<string, string>? Highlights { get; set; }
}

/// <summary>
/// Facet value for faceted search
/// </summary>
public sealed record FacetValue
{
    /// <summary>
    /// Facet value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Count of items with this facet value
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Is this facet currently selected/applied
    /// </summary>
    public bool IsSelected { get; set; }
}
