using CMS.Application.Common.Interfaces;
using CMS.Domain.Entities;
using CMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CMS.Infrastructure.Persistence;

/// <summary>
/// Initializes the database and seeds initial data.
/// </summary>
public sealed class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Ensures the database exists and applies pending migrations.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // First, ensure the database exists
            await EnsureDatabaseExistsAsync();

            // Then apply migrations
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    /// <summary>
    /// Creates the database if it doesn't exist.
    /// </summary>
    private async Task EnsureDatabaseExistsAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database name is not specified in the connection string.");
        }

        // Connect to the postgres system database to check/create the target database
        builder.Database = "postgres";
        var masterConnectionString = builder.ToString();

        await using var connection = new NpgsqlConnection(masterConnectionString);

        try
        {
            await connection.OpenAsync();

            // Check if database exists
            var checkDbQuery = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
            await using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
            checkCommand.Parameters.AddWithValue("@databaseName", databaseName);

            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                // Database doesn't exist, create it
                _logger.LogInformation("Database '{DatabaseName}' does not exist. Creating...", databaseName);

                // Cannot use parameters for database name in CREATE DATABASE
                // Use identifier quoting to prevent SQL injection
                var createDbQuery = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\" " +
                                   "WITH ENCODING = 'UTF8' " +
                                   "LC_COLLATE = 'en_US.UTF-8' " +
                                   "LC_CTYPE = 'en_US.UTF-8' " +
                                   "TEMPLATE = template0";

                await using var createCommand = new NpgsqlCommand(createDbQuery, connection);
                await createCommand.ExecuteNonQueryAsync();

                _logger.LogInformation("Database '{DatabaseName}' created successfully", databaseName);
            }
            else
            {
                _logger.LogInformation("Database '{DatabaseName}' already exists", databaseName);
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P04") // Database already exists
        {
            _logger.LogInformation("Database '{DatabaseName}' already exists (concurrent creation)", databaseName);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error while ensuring database exists: {SqlState}", ex.SqlState);
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
