using System.Text;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Constants;
using CMS.Domain.Enums;
using CMS.Infrastructure.Identity;
using CMS.Infrastructure.Persistence;
using CMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CMS.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitializer>();

        // Identity services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // HTTP Context accessor (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero // No tolerance for token expiration
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Support token from cookie for added security
                    if (context.Request.Cookies.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    // Add token expired header for client handling
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.RequireAdmin, policy =>
                policy.RequireRole(UserRole.Admin.ToString()))
            .AddPolicy(Permissions.RequireAdminOrDeveloper, policy =>
                policy.RequireRole(UserRole.Admin.ToString(), UserRole.Developer.ToString()))
            .AddPolicy(Permissions.CanAccessDashboard, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(UserRole.Admin.ToString()) ||
                    context.User.IsInRole(UserRole.Developer.ToString())))
            .AddPolicy(Permissions.CanAccessDesigner, policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole(UserRole.Admin.ToString()) ||
                    context.User.IsInRole(UserRole.Developer.ToString())))
            .AddPolicy(Permissions.CanManageUsers, policy =>
                policy.RequireRole(UserRole.Admin.ToString()));

        return services;
    }
}