using Dima.WorkflowAuditing.Data;
using Dima.WorkflowAuditing.Middleware;
using Dima.WorkFlowAuditMiddleware.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dima.WorkflowAuditing.Extensions;

public static class WorkflowAuditExtensions
{
    public static IServiceCollection AddWorkflowAuditing(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString));

        //services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }

    public static IApplicationBuilder UseWorkflowAuditing(this IApplicationBuilder app)
    {
        // Pure HTTP pipeline registration - no blocking DB calls
        return app.UseMiddleware<WorkflowAuditMiddleware>();
    }
}