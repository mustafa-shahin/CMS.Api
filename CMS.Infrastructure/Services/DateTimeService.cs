using CMS.Application.Common.Interfaces;

namespace CMS.Infrastructure.Services;

/// <summary>
/// Service providing current date and time for testability.
/// </summary>
public sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}