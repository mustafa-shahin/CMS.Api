using System.Linq.Expressions;

namespace CMS.Application.Common.Specifications;

/// <summary>
/// Specification pattern for building complex, reusable query logic
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Predicate expression to filter entities
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading related entities
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include strings for eager loading (for nested includes like "Author.Profile")
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Additional order by expressions (for ThenBy)
    /// </summary>
    List<Expression<Func<T, object>>> ThenBy { get; }

    /// <summary>
    /// Additional order by descending expressions (for ThenByDescending)
    /// </summary>
    List<Expression<Func<T, object>>> ThenByDescending { get; }

    /// <summary>
    /// Paging - number of records to skip
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Paging - number of records to take
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Enable split query for includes (to avoid cartesian explosion)
    /// </summary>
    bool IsSplitQuery { get; }

    /// <summary>
    /// Disable tracking for read-only queries
    /// </summary>
    bool IsNoTracking { get; }
}
