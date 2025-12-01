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
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}