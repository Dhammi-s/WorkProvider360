using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Sends transactional email over SMTP using settings from configuration.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toAddress);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
        };

        try
        {
            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {ToAddress} with subject {Subject}", toAddress, subject);
        }
        catch (Exception ex)
        {
            // Do not surface SMTP failures to the caller of forgot-password flows.
            _logger.LogError(ex, "Failed to send email to {ToAddress}", toAddress);
            throw;
        }
    }

    public Task SendPasswordResetAsync(string toAddress, string fullName, string resetLink, CancellationToken ct = default)
    {
        var body = $"""
            <p>Hi {WebUtility.HtmlEncode(fullName)},</p>
            <p>We received a request to reset your password. Click the link below to choose a new one.
            This link expires shortly and can be used only once.</p>
            <p><a href="{resetLink}">Reset your password</a></p>
            <p>If you did not request this, you can safely ignore this email.</p>
            """;

        return SendAsync(toAddress, "Reset your password", body, ct);
    }
}
