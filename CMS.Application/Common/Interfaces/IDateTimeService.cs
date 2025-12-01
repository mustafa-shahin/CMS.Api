namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Service for abstracting DateTime operations for testability.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}