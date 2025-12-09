using CMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CMS.Infrastructure.Persistence;

/// <summary>
/// Factory for creating ApplicationDbContext at design time (for migrations).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CMS.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("CMS.Infrastructure");
            });

        // Create mock services for design-time
        var currentUserService = new DesignTimeCurrentUserService();
        var dateTimeService = new DesignTimeDateTimeService();

        return new ApplicationDbContext(
            optionsBuilder.Options,
            currentUserService,
            dateTimeService);
    }

    /// <summary>
    /// Mock current user service for design-time.
    /// </summary>
    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public int? UserId => 1; // Default to user ID 1 for migrations
        public string? Email => "system@design-time.local";
        public string? Role => "System";
        public bool IsAuthenticated => false;
        public string? IpAddress => "127.0.0.1";
        public string? UserAgent => "EF Core Design-Time";
    }

    /// <summary>
    /// Mock date time service for design-time.
    /// </summary>
    private class DesignTimeDateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
    }
}
