namespace Antheia.Infrastructure;

using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Data;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Resolvers;
using Antheia.Infrastructure.Security;
using Antheia.Infrastructure.Services;
using Dima.ChangeAudit.Abstractions;
using Dima.ChangeAudit.Extensions;
using Dima.WorkFlowAuditMiddleware.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var legacyConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var connectionString = configuration.GetConnectionString("AntheiaConnection")
            ?? throw new InvalidOperationException("Connection string 'AntheiaConnection' not found.");

        // Database Context
        services.AddDbContext<LegacyMembershipDbContext>(options =>
            options.UseSqlServer(legacyConnectionString));

        // Add Antheia Application DbContext
        services.AddDbContext<AntheiaDbContext>((serviceProvider, options) =>
            options.UseSqlServer(connectionString).UseChangeAudit(serviceProvider));       

        // Add Change Audit Middleware
        services.AddAuditLogging(connectionString);
        services.AddScoped<IAuditHierarchyResolver, ApplicationAuditHierarchyResolver>();
        services.AddScoped<IAuditContainerResolver, AuditContainerResolver>();

        // Add Workflow Audit Middleware
        services.AddWorkflowAuditing(connectionString);

        services.AddHttpContextAccessor();

        // 2. Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // 3. Security & Infrastructure Utilities
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // 4. Application Service Implementations
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBlendService, BlendService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        

        return services;
    }
}