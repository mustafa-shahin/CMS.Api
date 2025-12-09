using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Common.Interfaces;

/// <summary>
/// Abstraction for the application database context.
/// Allows the Application layer to remain independent of infrastructure concerns.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Users table.
    /// </summary>
    DbSet<User> Users { get; }

    /// <summary>
    /// Refresh tokens table.
    /// </summary>
    DbSet<RefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Audit logs table.
    /// </summary>
    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Pages table.
    /// </summary>
    DbSet<Page> Pages { get; }

    /// <summary>
    /// Page versions table.
    /// </summary>
    DbSet<PageVersion> PageVersions { get; }

    /// <summary>
    /// Files table.
    /// </summary>
    DbSet<FileEntity> Files { get; }

    /// <summary>
    /// Images table.
    /// </summary>
    DbSet<ImageEntity> Images { get; }

    /// <summary>
    /// Folders table.
    /// </summary>
    DbSet<Folder> Folders { get; }

    /// <summary>
    /// Site configurations table.
    /// </summary>
    DbSet<SiteConfiguration> SiteConfigurations { get; }

    /// <summary>
    /// Customization settings table.
    /// </summary>
    DbSet<CustomizationSettings> CustomizationSettings { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}