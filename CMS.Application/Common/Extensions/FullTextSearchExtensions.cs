using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CMS.Application.Common.Extensions;

/// <summary>
/// Extension methods and helper utilities for PostgreSQL full-text search.
///
/// IMPORTANT NOTES:
/// ================
/// 1. The actual EF Core DbFunctions are in PostgreSqlFunctions.cs
/// 2. This file contains utility methods for sanitization and manual SQL generation
/// 3. Helper methods (SanitizeSearchQuery, BuildWeightedSearchVector) are real C# implementations
/// </summary>
public static class FullTextSearchExtensions
{

    /// <summary>
    /// Sanitizes user search input to prevent SQL injection and abuse.
    ///
    /// Security measures applied:
    /// - Removes single quotes (potential SQL injection vector)
    /// - Collapses multiple spaces to single space
    /// - Limits length to 200 characters (prevents abuse)
    /// - Trims whitespace
    ///
    /// This is called by SearchService before passing search terms to PostgreSQL.
    /// </summary>
    /// <param name="query">User-provided search query</param>
    /// <returns>Sanitized search query safe for use in SQL</returns>
    public static string SanitizeSearchQuery(this string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        // Remove potentially dangerous characters
        var sanitized = query.Trim();

        // Replace single quotes with spaces (prevents SQL injection attempts)
        sanitized = sanitized.Replace("'", " ");

        // Replace multiple spaces with single space (cleaner queries)
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", " ");

        // Limit length to prevent abuse (max 200 characters)
        if (sanitized.Length > 200)
        {
            sanitized = sanitized.Substring(0, 200);
        }

        return sanitized;
    }

    /// <summary>
    /// Builds a PostgreSQL weighted search vector SQL string for manual SQL generation.
    ///
    /// NOTE: This method is for MANUAL SQL string building, not EF Core LINQ queries.
    /// It's a utility for creating raw SQL strings, not a DbFunction.
    ///
    /// The current implementation uses Expression trees (see SearchService.cs) instead of
    /// manual SQL generation, so this method is NOT used in the main search functionality.
    ///
    /// Use case: If you need to write raw SQL queries with full-text search manually.
    ///
    /// Example output:
    /// setweight(to_tsvector('english', coalesce(field1, '')), 'A') ||
    /// setweight(to_tsvector('english', coalesce(field2, '')), 'B')
    /// </summary>
    /// <param name="fieldWeights">Dictionary of field names to weights (A, B, C, D)</param>
    /// <param name="language">PostgreSQL text search language (default: english)</param>
    /// <returns>SQL string for weighted search vector</returns>
    public static string BuildWeightedSearchVector(
        Dictionary<string, char> fieldWeights,
        string language = "english")
    {
        var sb = new StringBuilder();
        var first = true;

        foreach (var (field, weight) in fieldWeights)
        {
            if (!first)
            {
                sb.Append(" || ");
            }

            sb.Append($"setweight(to_tsvector('{language}', coalesce({field}, '')), '{weight}')");
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a PostgreSQL computed column expression for full-text search.
    ///
    /// NOTE: This method is for MANUAL SQL string building for migrations, not EF Core LINQ queries.
    /// It's used in migration files to create computed columns or generated indexes.
    ///
    /// The current implementation builds search vectors dynamically in queries (see SearchService.cs)
    /// rather than using computed columns, so this is primarily for migration/index creation.
    ///
    /// Use case: Creating full-text search indexes in migrations (see AddFullTextSearchIndexes.cs)
    ///
    /// Example output:
    /// setweight(to_tsvector('english', coalesce("Title", '')), 'A') ||
    /// setweight(to_tsvector('english', coalesce("Description", '')), 'B')
    /// </summary>
    /// <param name="weightedFields">Array of (field, weight) tuples</param>
    /// <returns>SQL string for search vector expression</returns>
    public static string CreateSearchVectorExpression(
        params (string field, char weight)[] weightedFields)
    {
        var sb = new StringBuilder();
        var first = true;

        foreach (var (field, weight) in weightedFields)
        {
            if (!first)
            {
                sb.Append(" || ");
            }

            sb.Append($"setweight(to_tsvector('english', coalesce(\"{field}\", '')), '{weight}')");
            first = false;
        }

        return sb.ToString();
    }
}

/// <summary>
/// Search configuration for an entity
/// </summary>
public sealed class SearchConfiguration<T> where T : class
{
    /// <summary>
    /// Searchable fields with their weights
    /// </summary>
    public Dictionary<string, char> SearchableFields { get; set; } = new();

    /// <summary>
    /// Filterable fields with allowed operators
    /// </summary>
    public Dictionary<string, HashSet<string>> FilterableFields { get; set; } = new();

    /// <summary>
    /// Sortable fields
    /// </summary>
    public HashSet<string> SortableFields { get; set; } = new();

    /// <summary>
    /// Default sort field
    /// </summary>
    public string? DefaultSortField { get; set; }

    /// <summary>
    /// Default sort direction
    /// </summary>
    public bool DefaultSortDescending { get; set; } = false;

    /// <summary>
    /// Maximum page size
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Default page size
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Language for full-text search
    /// </summary>
    public string Language { get; set; } = "english";

    /// <summary>
    /// Minimum search term length
    /// </summary>
    public int MinSearchTermLength { get; set; } = 2;

    /// <summary>
    /// Add searchable field
    /// </summary>
    public SearchConfiguration<T> AddSearchableField(
        Expression<Func<T, object>> field,
        char weight = 'D')
    {
        var propertyName = GetPropertyName(field);
        SearchableFields[propertyName] = weight;
        return this;
    }

    /// <summary>
    /// Add filterable field
    /// </summary>
    public SearchConfiguration<T> AddFilterableField(
        Expression<Func<T, object>> field,
        params string[] allowedOperators)
    {
        var propertyName = GetPropertyName(field);
        FilterableFields[propertyName] = allowedOperators.Length > 0
            ? new HashSet<string>(allowedOperators, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string> { "Equals" };
        return this;
    }

    /// <summary>
    /// Add sortable field
    /// </summary>
    public SearchConfiguration<T> AddSortableField(Expression<Func<T, object>> field)
    {
        var propertyName = GetPropertyName(field);
        SortableFields.Add(propertyName);
        return this;
    }

    /// <summary>
    /// Set default sort
    /// </summary>
    public SearchConfiguration<T> SetDefaultSort(
        Expression<Func<T, object>> field,
        bool descending = false)
    {
        DefaultSortField = GetPropertyName(field);
        DefaultSortDescending = descending;
        return this;
    }

    private static string GetPropertyName(Expression<Func<T, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression operand)
        {
            return operand.Member.Name;
        }

        throw new ArgumentException("Expression must be a property access", nameof(expression));
    }
}
