using CMS.Api.Middleware;
using CMS.Infrastructure.Persistence;

namespace CMS.Api.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder to configure the middleware pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the middleware pipeline for security and request handling.
    /// </summary>
    public static IApplicationBuilder UseSecurityPipeline(this IApplicationBuilder app)
    {
        // Security headers first
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Exception handling
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    /// <summary>
    /// Initializes the database with migrations and seed data.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initializer.InitializeAsync();
        await initializer.SeedAsync();
    }
}