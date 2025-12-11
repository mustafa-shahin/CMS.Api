namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Marker interface for entities that support advanced search functionality
/// </summary>
public interface ISearchableEntity
{
    /// <summary>
    /// Entity ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Get full-text search vector value (for PostgreSQL ts_vector)
    /// Override to provide custom search vector composition
    /// </summary>
    string GetSearchVector();
}
