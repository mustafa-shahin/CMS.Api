namespace CMS.Application.Common.Models.Search;

/// <summary>
/// Generic search request model supporting filtering, sorting, paging, and full-text search
/// </summary>
public record SearchRequest
{
    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Full-text search term
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Search fields to search in (if null, searches all searchable fields)
    /// </summary>
    public List<string>? SearchFields { get; set; }

    /// <summary>
    /// Filters to apply
    /// </summary>
    public List<FilterCriteria>? Filters { get; set; }

    /// <summary>
    /// Sort criteria (multiple fields supported)
    /// </summary>
    public List<SortCriteria>? Sorts { get; set; }

    /// <summary>
    /// Include deleted records (soft delete support)
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Minimum search relevance score (0.0 - 1.0) for full-text search
    /// </summary>
    public double? MinRelevanceScore { get; set; }
}

/// <summary>
/// Filter criteria for a single field
/// </summary>
public sealed record FilterCriteria
{
    /// <summary>
    /// Field name to filter on
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Filter operator
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Value to filter by
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Logical operator to combine with next filter (AND/OR)
    /// </summary>
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
}

/// <summary>
/// Sort criteria for a single field
/// </summary>
public sealed record SortCriteria
{
    /// <summary>
    /// Field name to sort by
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>
/// Filter operators
/// </summary>
public enum FilterOperator
{
    /// <summary>
    /// Equals (=)
    /// </summary>
    Equals,

    /// <summary>
    /// Not equals (!=)
    /// </summary>
    NotEquals,

    /// <summary>
    /// Contains (LIKE %value%)
    /// </summary>
    Contains,

    /// <summary>
    /// Does not contain (NOT LIKE %value%)
    /// </summary>
    NotContains,

    /// <summary>
    /// Starts with (LIKE value%)
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with (LIKE %value)
    /// </summary>
    EndsWith,

    /// <summary>
    /// Greater than (>)
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater than or equal (>=)
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Less than (<)
    /// </summary>
    LessThan,

    /// <summary>
    /// Less than or equal (<=)
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// In (IN (...))
    /// </summary>
    In,

    /// <summary>
    /// Not in (NOT IN (...))
    /// </summary>
    NotIn,

    /// <summary>
    /// Is null (IS NULL)
    /// </summary>
    IsNull,

    /// <summary>
    /// Is not null (IS NOT NULL)
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Between (BETWEEN x AND y)
    /// </summary>
    Between
}

/// <summary>
/// Logical operators for combining filters
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// Logical AND
    /// </summary>
    And,

    /// <summary>
    /// Logical OR
    /// </summary>
    Or
}

/// <summary>
/// Sort direction
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending order
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending order
    /// </summary>
    Descending
}
