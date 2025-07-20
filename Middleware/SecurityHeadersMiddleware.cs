using Saigor.Configuration;

namespace Saigor.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeaders _headers;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeaders headers)
    {
        _next = next;
        _headers = headers;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_headers.EnableXContentTypeOptions)
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        }

        if (_headers.EnableXFrameOptions)
        {
            context.Response.Headers.Append("X-Frame-Options", "DENY");
        }

        if (_headers.EnableXssProtection)
        {
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, SecurityHeaders headers)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(headers);
    }
} 