using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Dima.ChangeAudit.Data;

namespace Dima.ChangeAudit.Extensions;

public static class ChangeAuditServiceExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChangeAuditDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddScoped<AuditLogInterceptor>();
        return services;
    }
}