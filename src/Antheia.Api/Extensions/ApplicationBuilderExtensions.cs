using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Antheia.Api.Extensions;

public static partial class ApplicationBuilderExtensions
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;

                // 1. Extract CorrelationId set by CorrelationIdMiddleware
                var correlationId = context.Items[CorrelationIdHeaderName]?.ToString()
                                    ?? context.TraceIdentifier;

                if (exception != null)
                {
                    // 2. Resolve ILogger
                    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");

                    // 3. Log exception with CorrelationId context
                    logger.LogError(
                        exception,
                        "Unhandled exception occurred. CorrelationId: {CorrelationId} | Path: {Path}",
                        correlationId,
                        feature?.Path
                    );
                }

                // 4. Return structured error object to client
                await context.Response.WriteAsJsonAsync(new
                {
                    error = exception?.Message,
                    detail = exception?.InnerException?.Message,
                    correlationId = correlationId
                });
            });
        });
    }
}