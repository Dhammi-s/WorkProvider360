/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationQuestionRepository, ApplicationQuestionRepository>();
        services.AddScoped<IApplicationSettingsRepository, ApplicationSettingsRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<ISchedulingSettingsRepository, SchedulingSettingsRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<ITimezoneRepository, TimezoneRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();
        services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();
        services.AddScoped<ILogSettingsRepository, LogSettingsRepository>();
        services.AddScoped<IBrandingRepository, BrandingRepository>();
        services.AddScoped<ILoginContentRepository, LoginContentRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPosRepository, PosRepository>();

        return services;
    }
}
