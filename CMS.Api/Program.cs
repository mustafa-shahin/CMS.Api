using CMS.Api.Extensions;
using CMS.Application;
using CMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Service Registration
// ============================================

// Application layer services (MediatR, FluentValidation, Behaviors)
builder.Services.AddApplication();

// Infrastructure layer services (Database, Identity, Authentication)
builder.Services.AddInfrastructure(builder.Configuration);

// API layer services (Controllers, Versioning, OpenAPI, Authorization, CORS, Rate Limiting)
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// ============================================
// Middleware Pipeline
// ============================================

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}
else
{
    app.UseHsts();
}

// Security middleware (headers, logging, exception handling)
app.UseSecurityPipeline();

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("Default");

// Rate limiting
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

// ============================================
// Database Initialization
// ============================================

await app.InitializeDatabaseAsync();

// ============================================
// Run Application
// ============================================

app.Run();