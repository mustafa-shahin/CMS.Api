using System.Diagnostics;

namespace CMS.Api.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// Logs request details, timing, and response status.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        // Log request
        _logger.LogInformation(
            "Request {RequestId} started: {Method} {Path}{QueryString} from {RemoteIp}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            GetClientIpAddress(context));

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var level = context.Response.StatusCode >= 500 ? LogLevel.Error :
                        context.Response.StatusCode >= 400 ? LogLevel.Warning :
                        LogLevel.Information;

            _logger.Log(
                level,
                "Request {RequestId} completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {RequestId} {Method} {Path} took {ElapsedMs}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}