using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CMS.Infrastructure.Persistence;

/// <summary>
/// Initializes the database and seeds initial data.
/// </summary>
public sealed class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    /// <summary>
    /// Seeds initial data into the database.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Seed default admin user if no users exist
        if (!await _context.Users.AnyAsync())
        {
            var adminPassword = _passwordHasher.HashPassword("Admin@123!");

            var admin = User.Create(
                email: "admin@cms.local",
                passwordHash: adminPassword,
                firstName: "System",
                lastName: "Administrator",
                role: UserRole.Admin);

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default admin user created: admin@cms.local");
        }
    }
}