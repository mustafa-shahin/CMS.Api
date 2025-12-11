using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Application.Common.Extensions;

/// <summary>
/// PostgreSQL full-text search functions for EF Core DbFunction mapping.
///
/// IMPORTANT: These methods are NOT meant to be called directly in C# code!
/// They are EF Core DbFunctions that exist only to be translated to SQL.
///
/// How this works:
/// 1. These methods are declared with signatures matching PostgreSQL functions
/// 2. In ApplicationDbContext.OnModelCreating, they are registered via HasDbFunction
/// 3. When used in LINQ queries, EF Core translates them to actual PostgreSQL SQL
/// 4. The C# method bodies are NEVER executed - hence the InvalidOperationException
///
/// Example usage (correct):
/// <code>
/// var query = dbContext.Users.Where(u =>
///     PostgreSqlFunctions.Matches(
///         PostgreSqlFunctions.ToTsVector("english", u.Email),
///         PostgreSqlFunctions.WebSearchToTsQuery("english", "search term")
///     ));
/// // EF Core translates this to SQL:
/// // WHERE to_tsvector('english', "Email") @@ websearch_to_tsquery('english', 'search term')
/// </code>
///
/// Example (WRONG - will throw exception):
/// <code>
/// var result = PostgreSqlFunctions.ToTsVector("english", "text"); // DON'T DO THIS!
/// </code>
///
/// This is the standard EF Core pattern for custom database functions, similar to:
/// - EF.Functions.Like()
/// - EF.Functions.DateDiffDay()
/// - Npgsql's EF.Functions.ILike()
/// </summary>
public static class PostgreSqlFunctions
{
    /// <summary>
    /// PostgreSQL to_tsvector function - converts text to a tsvector for full-text search.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: to_tsvector(config, text)
    /// Example: to_tsvector('english', 'The quick brown fox')
    /// </summary>
    /// <param name="config">Text search configuration (e.g., 'english', 'simple')</param>
    /// <param name="text">Text to convert to tsvector</param>
    /// <returns>tsvector representation of the text</returns>
    public static string ToTsVector(string config, string text)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code. " +
            "Use it in LINQ expressions like: query.Where(x => PostgreSqlFunctions.ToTsVector('english', x.Field))");
    }

    /// <summary>
    /// PostgreSQL plainto_tsquery function - converts plain text to a tsquery.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: plainto_tsquery(config, querytext)
    /// Example: plainto_tsquery('english', 'quick brown')
    /// </summary>
    /// <param name="config">Text search configuration (e.g., 'english', 'simple')</param>
    /// <param name="query">Query text to convert to tsquery</param>
    /// <returns>tsquery representation of the text</returns>
    public static string PlainToTsQuery(string config, string query)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL websearch_to_tsquery function - converts web search syntax to a tsquery.
    /// Supports quoted phrases, AND/OR operators, and negation with minus sign.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: websearch_to_tsquery(config, querytext)
    /// Example: websearch_to_tsquery('english', '"brown fox" OR quick')
    /// </summary>
    /// <param name="config">Text search configuration (e.g., 'english', 'simple')</param>
    /// <param name="query">Web search query text</param>
    /// <returns>tsquery representation optimized for web search</returns>
    public static string WebSearchToTsQuery(string config, string query)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL ts_rank function - ranks search results by relevance.
    /// Returns a float value representing how well the document matches the query.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: ts_rank(vector, query)
    /// Example: ts_rank(to_tsvector('english', text), websearch_to_tsquery('english', 'search'))
    /// Returns: Float value (higher = better match)
    /// </summary>
    /// <param name="vector">tsvector to rank against</param>
    /// <param name="query">tsquery to match</param>
    /// <returns>Relevance score (higher is more relevant)</returns>
    public static double TsRank(string vector, string query)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL @@ operator wrapper - checks if tsvector matches tsquery.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: vector @@ query (via tsvector_matches wrapper function)
    /// Example: to_tsvector('english', text) @@ websearch_to_tsquery('english', 'search')
    /// Returns: Boolean (true if matches)
    ///
    /// Note: We use a wrapper function tsvector_matches(vector, query) in PostgreSQL
    /// because EF Core cannot directly map to the @@ operator.
    /// </summary>
    /// <param name="vector">tsvector to match against</param>
    /// <param name="query">tsquery to match</param>
    /// <returns>True if vector matches query</returns>
    public static bool Matches(string vector, string query)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL || operator wrapper for concatenating tsvectors.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: vector1 || vector2 (via concat_tsvectors wrapper function)
    /// Example: to_tsvector('english', field1) || to_tsvector('english', field2)
    ///
    /// Note: We use a wrapper function concat_tsvectors(vector1, vector2) in PostgreSQL
    /// because EF Core cannot directly map to the || operator.
    /// </summary>
    /// <param name="vector1">First tsvector</param>
    /// <param name="vector2">Second tsvector</param>
    /// <returns>Concatenated tsvector</returns>
    public static string ConcatTsVectors(string vector1, string vector2)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL setweight function - assigns a weight (A, B, C, or D) to a tsvector.
    /// Weights are used in ranking to prioritize certain fields over others.
    /// A = highest weight, D = lowest weight.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: setweight(vector, weight)
    /// Example: setweight(to_tsvector('english', title), 'A')
    /// </summary>
    /// <param name="vector">tsvector to assign weight to</param>
    /// <param name="weight">Weight label ('A', 'B', 'C', or 'D')</param>
    /// <returns>Weighted tsvector</returns>
    public static string SetWeight(string vector, string weight)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }

    /// <summary>
    /// PostgreSQL coalesce function - returns the first non-null value.
    ///
    /// IMPORTANT: This method is NOT implemented in C# and will throw an exception if called directly.
    /// It is only meant to be used in LINQ queries where EF Core will translate it to SQL.
    ///
    /// SQL Translation: coalesce(value, defaultValue)
    /// Example: coalesce(description, '')
    /// </summary>
    /// <param name="value">Value to check</param>
    /// <param name="defaultValue">Default value if first is null</param>
    /// <returns>First non-null value</returns>
    public static string Coalesce(string value, string defaultValue)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core. " +
            "It should NEVER be called directly in C# code.");
    }
}
