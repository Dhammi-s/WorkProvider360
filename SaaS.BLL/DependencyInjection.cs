using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaS.BLL.Security;
using SaaS.BLL.Services;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the business-logic layer: settings, security primitives and
    /// application services. Call <c>AddDataAccess</c> as well.
    /// </summary>
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));

        services.AddSingleton<IPasswordHasher, Sha512PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IBrandingService, BrandingService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IInvoiceService, InvoiceService>();

        return services;
    }
}
