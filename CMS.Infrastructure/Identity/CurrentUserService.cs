using System.Security.Claims;
using CMS.Application.Common.Interfaces;
using CMS.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace CMS.Infrastructure.Identity;

/// <summary>
/// Service for accessing current authenticated user information from HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(Permissions.UserIdClaimType);

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(Permissions.EmailClaimType);

    public string? Role => _httpContextAccessor.HttpContext?.User
        .FindFirstValue(Permissions.RoleClaimType);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        .Identity?.IsAuthenticated ?? false;

    public string? IpAddress => GetIpAddress();

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request
        .Headers.UserAgent.ToString();

    private string? GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null) return null;

        // Check for forwarded IP (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the list (original client IP)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}