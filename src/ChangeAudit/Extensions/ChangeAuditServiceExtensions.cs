using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Data;
using Dima.ChangeAudit.Interceptors;
using Dima.ChangeAudit.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IAuditLogService, AuditLogService>();
        return services;
    }
}