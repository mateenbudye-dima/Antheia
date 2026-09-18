namespace Antheia.Infrastructure;

using Antheia.Application.Interfaces;
using Antheia.Infrastructure.Data;
using Antheia.Infrastructure.Repositories;
using Antheia.Infrastructure.Security;
using Antheia.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Database Context
        services.AddDbContext<LegacyMembershipDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add Antheia Application DbContext
        services.AddDbContext<AntheiaDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AntheiaConnection")));

        services.AddHttpContextAccessor();

        // 2. Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // 3. Security & Infrastructure Utilities
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // 4. Application Service Implementations
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}