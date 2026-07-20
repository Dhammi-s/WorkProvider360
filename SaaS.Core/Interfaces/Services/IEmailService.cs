namespace SaaS.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);

    Task SendPasswordResetAsync(string toAddress, string fullName, string resetLink, CancellationToken ct = default);
}
