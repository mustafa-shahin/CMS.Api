using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;

namespace CMS.Api.Attributes;

/// <summary>
/// Rate limiting attribute to prevent API abuse
/// Limits the number of requests per time window per user/IP
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private static readonly ConcurrentDictionary<string, RateLimitEntry> RateLimitCache = new();
    private static readonly object CleanupLock = new();
    private static DateTime _lastCleanup = DateTime.UtcNow;

    /// <summary>
    /// Maximum number of requests allowed
    /// </summary>
    public int Requests { get; set; } = 10;

    /// <summary>
    /// Time window in minutes
    /// </summary>
    public int PerMinutes { get; set; } = 1;

    /// <summary>
    /// Custom error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Get identifier (user ID if authenticated, otherwise IP address)
        var identifier = GetClientIdentifier(context);

        // Get the cache key
        var cacheKey = $"{identifier}:{context.ActionDescriptor.DisplayName}";

        // Clean up old entries periodically (every 5 minutes)
        CleanupOldEntries();

        // Get or create rate limit entry
        var entry = RateLimitCache.GetOrAdd(cacheKey, _ => new RateLimitEntry
        {
            WindowStart = DateTime.UtcNow,
            RequestCount = 0
        });

        lock (entry)
        {
            var windowEnd = entry.WindowStart.AddMinutes(PerMinutes);
            var now = DateTime.UtcNow;

            // Check if we're still in the same time window
            if (now > windowEnd)
            {
                // Start new window
                entry.WindowStart = now;
                entry.RequestCount = 0;
            }

            // Increment request count
            entry.RequestCount++;

            // Check if limit exceeded
            if (entry.RequestCount > Requests)
            {
                var retryAfter = (windowEnd - now).TotalSeconds;

                context.HttpContext.Response.Headers["Retry-After"] = ((int)Math.Ceiling(retryAfter)).ToString();

                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = ErrorMessage ?? $"Rate limit exceeded. Maximum {Requests} requests per {PerMinutes} minute(s). Please try again later.",
                    retryAfter = Math.Ceiling(retryAfter)
                })
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };

                return;
            }
        }

        base.OnActionExecuting(context);
    }

    /// <summary>
    /// Get client identifier (User ID or IP address)
    /// </summary>
    private string GetClientIdentifier(ActionExecutingContext context)
    {
        // Try to get user ID from claims
        var userId = context.HttpContext.User?.FindFirst("sub")?.Value
            ?? context.HttpContext.User?.FindFirst("userId")?.Value
            ?? context.HttpContext.User?.FindFirst("id")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? "unknown";

        return $"ip:{ipAddress}";
    }

    /// <summary>
    /// Clean up old entries from cache to prevent memory leaks
    /// </summary>
    private void CleanupOldEntries()
    {
        if ((DateTime.UtcNow - _lastCleanup).TotalMinutes < 5)
        {
            return; // Only cleanup every 5 minutes
        }

        lock (CleanupLock)
        {
            if ((DateTime.UtcNow - _lastCleanup).TotalMinutes < 5)
            {
                return; // Double-check after acquiring lock
            }

            var now = DateTime.UtcNow;
            var keysToRemove = new List<string>();

            foreach (var kvp in RateLimitCache)
            {
                lock (kvp.Value)
                {
                    // Remove entries older than 1 hour
                    if ((now - kvp.Value.WindowStart).TotalHours > 1)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                RateLimitCache.TryRemove(key, out _);
            }

            _lastCleanup = now;
        }
    }

    /// <summary>
    /// Rate limit entry tracking request count and time window
    /// </summary>
    private class RateLimitEntry
    {
        public DateTime WindowStart { get; set; }
        public int RequestCount { get; set; }
    }
}
