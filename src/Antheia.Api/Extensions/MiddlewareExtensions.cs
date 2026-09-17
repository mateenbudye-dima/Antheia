namespace Antheia.Api.Extensions;

using Antheia.Api.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}