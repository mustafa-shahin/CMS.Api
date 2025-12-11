using CMS.Application.Common.Interfaces;
using CMS.Application.Common.Extensions;
using CMS.Domain.Common;
using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the CMS application.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    // Core entities
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // CMS entities
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageVersion> PageVersions => Set<PageVersion>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<ImageEntity> Images => Set<ImageEntity>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<SiteConfiguration> SiteConfigurations => Set<SiteConfiguration>();
    public DbSet<CustomizationSettings> CustomizationSettings => Set<CustomizationSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure PostgreSQL full-text search functions
        ConfigurePostgreSqlFunctions(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Configures PostgreSQL full-text search functions for EF Core DbFunction mapping.
    ///
    /// CRITICAL CONCEPT: EF Core DbFunctions
    /// =====================================
    /// These configurations tell EF Core how to translate C# method calls to PostgreSQL SQL functions.
    ///
    /// How it works:
    /// 1. We have C# methods in PostgreSqlFunctions.cs (e.g., ToTsVector, TsRank)
    /// 2. These methods throw exceptions and are NEVER executed in C#
    /// 3. We register them here using HasDbFunction, mapping them to PostgreSQL function names
    /// 4. When EF Core sees these methods in LINQ queries, it translates them to SQL
    /// 5. The actual implementation happens in PostgreSQL, not in C#
    ///
    /// Example:
    /// C# LINQ: query.Where(u => PostgreSqlFunctions.ToTsVector("english", u.Email))
    /// SQL Output: WHERE to_tsvector('english', "Email")
    ///
    /// The C# method is NEVER called - it's just a marker for EF Core to generate SQL.
    /// This is the same pattern used by EF.Functions.Like(), EF.Functions.DateDiffDay(), etc.
    /// </summary>
    private static void ConfigurePostgreSqlFunctions(ModelBuilder modelBuilder)
    {
        // Register PostgreSQL to_tsvector function
        // Maps: PostgreSqlFunctions.ToTsVector(config, text) -> to_tsvector(config, text) in SQL
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ToTsVector))!)
            .HasName("to_tsvector")    // Actual PostgreSQL function name
            .HasSchema(null);           // No schema (built-in PostgreSQL function)

        // Register PostgreSQL plainto_tsquery function
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.PlainToTsQuery))!)
            .HasName("plainto_tsquery")
            .HasSchema(null);

        // Register PostgreSQL websearch_to_tsquery function
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.WebSearchToTsQuery))!)
            .HasName("websearch_to_tsquery")
            .HasSchema(null);

        // Register PostgreSQL ts_rank function
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.TsRank))!)
            .HasName("ts_rank")
            .HasSchema(null);

        // Register PostgreSQL setweight function
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.SetWeight))!)
            .HasName("setweight")
            .HasSchema(null);

        // Register PostgreSQL coalesce function
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Coalesce))!)
            .HasName("coalesce")
            .HasSchema(null);

        // Register PostgreSQL tsvector concatenation wrapper function (wraps || operator)
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ConcatTsVectors))!)
            .HasName("concat_tsvectors")
            .HasSchema(null);

        // Register PostgreSQL tsvector matches wrapper function (wraps @@ operator)
        modelBuilder.HasDbFunction(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Matches))!)
            .HasName("tsvector_matches")
            .HasSchema(null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle audit fields
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTimeService.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _currentUserService.UserId;
                    entry.Entity.LastModifiedAt = _dateTimeService.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}