using Dima.ChangeAudit.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dima.ChangeAudit.Extensions;

public static class ChangeAuditApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> UseChangeAuditDatabaseInitializationAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var auditDbContext = scope.ServiceProvider.GetRequiredService<ChangeAuditDbContext>();

        if(auditDbContext != null)
        {
            // Runs the T-SQL initialization script safely
            await ChangeAuditDbInitializer.InitializeAsync(auditDbContext);
        }
        else
        {
            throw new InvalidOperationException("ChangeAuditDbContext is not registered in the service provider.");
        }

        return app;
    }
}