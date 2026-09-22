using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Antheia.Application.Exceptions;
using System.Net;

namespace Antheia.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items[CorrelationIdHeaderName]?.ToString() ?? context.TraceIdentifier;

            // Map known application exceptions to appropriate status codes
            int status = (int)HttpStatusCode.InternalServerError;
            object? responsePayload = null;

            switch (ex)
            {
                case NotFoundException nf:
                    status = StatusCodes.Status404NotFound;
                    responsePayload = new { error = nf.Message, correlationId };
                    break;
                case ValidationException vf:
                    status = StatusCodes.Status400BadRequest;
                    responsePayload = new { error = vf.Message, errors = vf.Errors, correlationId };
                    break;
                case ConflictException cf:
                    status = StatusCodes.Status409Conflict;
                    responsePayload = new { error = cf.Message, correlationId };
                    break;
                case UnauthorizedAccessException ua:
                    status = StatusCodes.Status401Unauthorized;
                    responsePayload = new { error = ua.Message, correlationId };
                    break;
                default:
                    // unexpected errors
                    responsePayload = new { error = "An unexpected error occurred.", detail = ex.Message, correlationId };
                    break;
            }

            _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId} | Path: {Path}", correlationId, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(responsePayload);
            }
            else
            {
                // If response already started, there's little we can do. Re-throw to let server handle it.
                throw;
            }
        }
    }
}
