using Asp.Versioning;
using CMS.Domain.Constants;
using CMS.Domain.Enums;
using Microsoft.OpenApi;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace CMS.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure API services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API layer services including controllers, versioning, OpenAPI, and authorization.
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure controllers with JSON options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // OpenAPI/Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CMS API",
                Version = "v1",
                Description = "Dynamic CMS Platform API - Secure, Scalable, Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "CMS Team",
                    Email = "support@cms.local"
                }
            });

            // JWT Bearer security definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Security requirement using Swashbuckle
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        // Authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.RequireAdmin, policy =>
                policy.RequireRole(UserRole.Admin.ToString()))
            .AddPolicy(Permissions.RequireAdminOrDeveloper, policy =>
                policy.RequireRole(UserRole.Admin.ToString(), UserRole.Developer.ToString()))
            .AddPolicy(Permissions.CanAccessDashboard, policy =>
                policy.RequireRole(UserRole.Admin.ToString(), UserRole.Developer.ToString()))
            .AddPolicy(Permissions.CanAccessDesigner, policy =>
                policy.RequireRole(UserRole.Admin.ToString(), UserRole.Developer.ToString()))
            .AddPolicy(Permissions.CanManageUsers, policy =>
                policy.RequireRole(UserRole.Admin.ToString()));

        // CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .WithHeaders("Authorization", "Content-Type", "X-Requested-With", "X-Api-Version")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global rate limit
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientId, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 100),
                    Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Window", 60))
                });
            });

            // Auth endpoints - stricter limits
            options.AddPolicy("auth", context =>
            {
                var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(clientId, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1)
                });
            });
        });

        // Health checks
        services.AddHealthChecks();

        // HTTP context accessor for CurrentUserService
        services.AddHttpContextAccessor();

        return services;
    }
}