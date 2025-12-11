using CMS.Application.Common.Models.Search;

namespace CMS.Application.Common.Attributes;

/// <summary>
/// Marks a property as searchable via full-text search
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SearchableAttribute : Attribute
{
    /// <summary>
    /// Search weight (A = highest, D = lowest) for PostgreSQL full-text search
    /// </summary>
    public SearchWeight Weight { get; set; } = SearchWeight.D;

    /// <summary>
    /// Include in default search (when no specific fields are specified)
    /// </summary>
    public bool IncludeInDefaultSearch { get; set; } = true;

    public SearchableAttribute()
    {
    }

    public SearchableAttribute(SearchWeight weight)
    {
        Weight = weight;
    }
}

/// <summary>
/// Marks a property as filterable
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FilterableAttribute : Attribute
{
    /// <summary>
    /// Allowed filter operators for this property
    /// </summary>
    public FilterOperator[] AllowedOperators { get; set; }

    /// <summary>
    /// Include in faceted search results
    /// </summary>
    public bool EnableFaceting { get; set; } = false;

    public FilterableAttribute(params FilterOperator[] allowedOperators)
    {
        AllowedOperators = allowedOperators.Length > 0
            ? allowedOperators
            : new[] { FilterOperator.Equals }; // Default to Equals if none specified
    }
}

/// <summary>
/// Marks a property as sortable
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SortableAttribute : Attribute
{
    /// <summary>
    /// Default sort direction if this field is used as default sort
    /// </summary>
    public SortDirection DefaultDirection { get; set; } = SortDirection.Ascending;

    public SortableAttribute()
    {
    }

    public SortableAttribute(SortDirection defaultDirection)
    {
        DefaultDirection = defaultDirection;
    }
}

/// <summary>
/// Search weight for full-text search (PostgreSQL setweight)
/// </summary>
public enum SearchWeight
{
    /// <summary>
    /// Highest weight (most important)
    /// </summary>
    A,

    /// <summary>
    /// High weight
    /// </summary>
    B,

    /// <summary>
    /// Medium weight
    /// </summary>
    C,

    /// <summary>
    /// Low weight (least important)
    /// </summary>
    D
}
