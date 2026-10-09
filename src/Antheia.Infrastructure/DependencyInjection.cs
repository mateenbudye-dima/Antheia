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
using Microsoft.Extensions.Logging;

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
        // Register an interceptor for cache invalidation and add it to DbContext
        services.Configure<UserCacheOptions>(configuration.GetSection("UserCache"));
        services.AddSingleton<UserProfileCacheService>();
        services.AddSingleton<UserCacheInvalidationInterceptor>();

        services.AddDbContext<LegacyMembershipDbContext>((serviceProvider, options) =>
            options.UseSqlServer(legacyConnectionString)
                   .AddInterceptors(serviceProvider.GetRequiredService<UserCacheInvalidationInterceptor>()));

        // Add Antheia Application DbContext
        services.AddDbContext<AntheiaDbContext>((serviceProvider, options) =>
            options.UseSqlServer(connectionString)
            .UseChangeAudit(serviceProvider));       

        // Add Change Audit Middleware
        services.AddAuditLogging(connectionString);
        services.AddScoped<IAuditHierarchyResolver, ApplicationAuditHierarchyResolver>();
        services.AddScoped<IAuditContainerResolver, AuditContainerResolver>();

        // Add Workflow Audit Middleware
        services.AddWorkflowAuditing(connectionString);

        services.AddHttpContextAccessor();

        // Add StackExchange Redis distributed cache (configuration: Redis:Configuration)
        var redisConfiguration = configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";
        var redisInstance = configuration.GetValue<string>("Redis:InstanceName") ?? "Antheia:";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfiguration;
            options.InstanceName = redisInstance;
        });

        // 2. Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // 3. Caching - user cache services
        services.AddSingleton<IUserProfileCacheService>(sp => sp.GetRequiredService<UserProfileCacheService>());

        // 3. Security & Infrastructure Utilities
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // 4. Application Service Implementations
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBlendService, BlendService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        

        return services;
    }
}