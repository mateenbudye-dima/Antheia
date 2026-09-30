using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Dima.WorkFlowAuditMiddleware.Data;

namespace WorkFlowAuditMiddleware.Extensions
{
    public static class WorkflowAuditApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> UseWorkflowAuditDatabaseInitializationAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var auditDbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            if (auditDbContext != null)
            {
                // Runs the T-SQL initialization script safely
                await AuditDbInitializer.InitializeAsync(auditDbContext);
            }
            else
            {
                throw new InvalidOperationException("ChangeAuditDbContext is not registered in the service provider.");
            }

            return app;
        }
    }
}
