namespace CMS.Api.Middleware;

/// <summary>
/// Middleware that adds security headers to all responses.
/// These headers help protect against common web vulnerabilities.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server header to prevent information disclosure
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Prevent clickjacking attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Enable browser's XSS filter (legacy but still useful)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Control referrer information sent to other sites
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy - strict policy for API
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'none'; " +
            "frame-ancestors 'none'; " +
            "form-action 'none'; " +
            "base-uri 'self'");

        // Permissions policy (formerly Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), " +
            "camera=(), " +
            "geolocation=(), " +
            "gyroscope=(), " +
            "magnetometer=(), " +
            "microphone=(), " +
            "payment=(), " +
            "usb=()");

        // Prevent caching of sensitive information
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }

        await _next(context);
    }
}