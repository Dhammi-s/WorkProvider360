using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Settings;
using SaaS.DAL.Infrastructure;
using SaaS.DAL.Repositories;

namespace SaaS.DAL;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the data-access layer: master DB settings, tenant context,
    /// connection factory, tenant resolver and all repositories.
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MasterDbSettings>(configuration.GetSection(MasterDbSettings.SectionName));

        // Per-request tenant state and connections.
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<ITenantResolver, TenantResolver>();

        // Repositories.
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        return services;
    }
}
