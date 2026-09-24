
using Dima.WorkflowAuditing.Attributes;
using Dima.WorkflowAuditing.Data;
using Dima.WorkflowAuditing.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Dima.WorkflowAuditing.Middleware;

public class WorkflowAuditMiddleware
{
    private readonly RequestDelegate _next;

    public WorkflowAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AuditDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // --- 1. Internal Built-in Endpoints Handled Directly by Middleware ---
        if (path.Equals("/api/audit/logs", StringComparison.OrdinalIgnoreCase))
        {
            if (HttpMethods.IsGet(context.Request.Method))
            {
                var logs = dbContext.AuditLogs.OrderByDescending(x => x.TimestampUtc).Take(100).ToList();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(logs));
                return;
            }
            if (HttpMethods.IsPost(context.Request.Method))
            {
                var customLog = await JsonSerializer.DeserializeAsync<AuditLog>(context.Request.Body);
                if (customLog != null)
                {
                    dbContext.AuditLogs.Add(customLog);
                    await dbContext.SaveChangesAsync();
                    context.Response.StatusCode = StatusCodes.Status201Created;
                    await context.Response.WriteAsJsonAsync(customLog);
                    return;
                }
            }
        }

        // --- 2. Middleware Audit Interception Logic for Application Endpoints ---
        var endpoint = context.GetEndpoint();
        var auditAttribute = endpoint?.Metadata.GetMetadata<AuditWorkflowAttribute>();

        if (auditAttribute == null || HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var user = context.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value
                     ?? "Anonymous";

        var userRoles = string.Join(",", user.FindAll(ClaimTypes.Role).Select(r => r.Value));

        bool isApprovalAction = path.Contains("/approve", StringComparison.OrdinalIgnoreCase);

        if (isApprovalAction && !user.IsInRole(auditAttribute.RequiredApprovalRole))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                Error = $"Forbidden: Action requires '{auditAttribute.RequiredApprovalRole}' role."
            });
            return;
        }

        await _next(context);

        if (context.Response.StatusCode is >= 200 and < 300)
        {
            var entityId = context.Request.RouteValues["id"]?.ToString() ?? "N/A";
            var projectId = context.Request.Headers["X-Project-Id"].FirstOrDefault() ?? "DefaultProject";

            var auditEntry = new AuditLog
            {
                ProjectId = projectId,
                EntityType = auditAttribute.EntityType,
                EntityId = entityId,
                Action = isApprovalAction ? "APPROVE" : context.Request.Method,
                UserId = userId,
                UserRoles = string.IsNullOrEmpty(userRoles) ? "None" : userRoles,
                Path = path,
                HttpMethod = context.Request.Method,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            dbContext.AuditLogs.Add(auditEntry);
            await dbContext.SaveChangesAsync();
        }
    }
}