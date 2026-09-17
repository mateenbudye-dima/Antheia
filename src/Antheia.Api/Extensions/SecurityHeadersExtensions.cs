namespace Antheia.Api.Extensions;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Prevent MIME-type sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Prevent the page from being embedded in frames
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Control how much referrer information is sent
            context.Response.Headers["Referrer-Policy"] =
                "strict-origin-when-cross-origin";

            // Restrict browser features
            context.Response.Headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            await next();
        });

        return app;
    }
}