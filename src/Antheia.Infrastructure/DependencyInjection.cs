namespace Antheia.Infrastructure;

using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Data;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Security;
using Antheia.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dima.WorkFlowAuditMiddleware.Extensions;

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

        // 1. Database Context
        services.AddDbContext<LegacyMembershipDbContext>(options =>
            options.UseSqlServer(legacyConnectionString));

        // Add Antheia Application DbContext
        services.AddDbContext<AntheiaDbContext>(options =>
            options.UseSqlServer(connectionString));

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