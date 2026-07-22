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

    public Task SendApplicationReceivedAsync(string toAddress, string fullName, string roleName, CancellationToken ct = default)
    {
        var body = $"""
            <p>Hi {WebUtility.HtmlEncode(fullName)},</p>
            <p>Thanks for applying for <strong>{WebUtility.HtmlEncode(roleName)}</strong> access on WorkProvider360.
            We've received your application and our team will review it shortly.</p>
            <p>You'll get another email once a decision has been made.</p>
            """;

        return SendAsync(toAddress, "We received your application", body, ct);
    }

    public Task SendApplicationNotificationAsync(string toAddress, string applicantName, string applicantEmail, string roleName, CancellationToken ct = default)
    {
        var body = $"""
            <p>A new role application has been submitted.</p>
            <ul>
              <li><strong>Name:</strong> {WebUtility.HtmlEncode(applicantName)}</li>
              <li><strong>Email:</strong> {WebUtility.HtmlEncode(applicantEmail)}</li>
              <li><strong>Requested role:</strong> {WebUtility.HtmlEncode(roleName)}</li>
            </ul>
            <p>Sign in to the dashboard to review and approve or reject it.</p>
            """;

        return SendAsync(toAddress, $"New {roleName} application from {applicantName}", body, ct);
    }

    public Task SendCredentialsAsync(string toAddress, string fullName, string email, string temporaryPassword, string loginUrl, CancellationToken ct = default)
    {
        var body = $"""
            <p>Hi {WebUtility.HtmlEncode(fullName)},</p>
            <p>Good news — your application has been <strong>approved</strong>. Your account is ready.</p>
            <ul>
              <li><strong>Email:</strong> {WebUtility.HtmlEncode(email)}</li>
              <li><strong>Temporary password:</strong> {WebUtility.HtmlEncode(temporaryPassword)}</li>
            </ul>
            <p><a href="{loginUrl}">Sign in here</a> and change your password from your profile as soon as possible.</p>
            """;

        return SendAsync(toAddress, "Your application was approved", body, ct);
    }

    public Task SendApplicationRejectedAsync(string toAddress, string fullName, string roleName, string reason, CancellationToken ct = default)
    {
        var body = $"""
            <p>Hi {WebUtility.HtmlEncode(fullName)},</p>
            <p>Thank you for your interest in <strong>{WebUtility.HtmlEncode(roleName)}</strong> access on WorkProvider360.
            After review, we're unable to approve your application at this time.</p>
            <p><strong>Reason:</strong> {WebUtility.HtmlEncode(reason)}</p>
            <p>You're welcome to reach out if you have any questions.</p>
            """;

        return SendAsync(toAddress, "Update on your application", body, ct);
    }

    public Task SendScheduleAssignedAsync(string toAddress, string userName, string title, string? location, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var body = $"""
            <p>Hi {WebUtility.HtmlEncode(userName)},</p>
            <p>You've been assigned a new schedule on WorkProvider360.</p>
            <ul>
              <li><strong>Job:</strong> {WebUtility.HtmlEncode(title)}</li>
              <li><strong>Location:</strong> {WebUtility.HtmlEncode(location ?? "—")}</li>
              <li><strong>Start:</strong> {FormatUtc(startUtc)}</li>
              <li><strong>End:</strong> {FormatUtc(endUtc)}</li>
            </ul>
            <p>Sign in to accept or reject it, add notes, and track your time.</p>
            """;

        return SendAsync(toAddress, $"New schedule: {title}", body, ct);
    }

    public Task SendScheduleNotificationAsync(string toAddress, string title, string assignedUserName, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var body = $"""
            <p>A schedule has been created.</p>
            <ul>
              <li><strong>Job:</strong> {WebUtility.HtmlEncode(title)}</li>
              <li><strong>Assigned to:</strong> {WebUtility.HtmlEncode(assignedUserName)}</li>
              <li><strong>Start:</strong> {FormatUtc(startUtc)}</li>
              <li><strong>End:</strong> {FormatUtc(endUtc)}</li>
            </ul>
            <p>Sign in to the dashboard to view the scheduler.</p>
            """;

        return SendAsync(toAddress, $"Schedule created: {title}", body, ct);
    }

    public Task SendScheduleInjuryReportAsync(string toAddress, string title, string reporterName, string message, CancellationToken ct = default)
    {
        var body = $"""
            <p><strong>An injury has been reported on a schedule.</strong></p>
            <ul>
              <li><strong>Job:</strong> {WebUtility.HtmlEncode(title)}</li>
              <li><strong>Reported by:</strong> {WebUtility.HtmlEncode(reporterName)}</li>
            </ul>
            <p><strong>Details:</strong> {WebUtility.HtmlEncode(message)}</p>
            <p>Please follow up as appropriate.</p>
            """;

        return SendAsync(toAddress, $"Injury reported: {title}", body, ct);
    }

    private static string FormatUtc(DateTime utc) =>
        utc.ToString("ddd, dd MMM yyyy HH:mm") + " UTC";
}
