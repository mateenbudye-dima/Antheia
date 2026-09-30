using Dima.ChangeAudit.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dima.ChangeAudit.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Extension method to easily attach the Audit Interceptor to any main application DbContext
    /// </summary>
    public static DbContextOptionsBuilder UseChangeAudit(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var interceptor = serviceProvider.GetRequiredService<AuditLogInterceptor>();
        return optionsBuilder.AddInterceptors(interceptor);
    }
}