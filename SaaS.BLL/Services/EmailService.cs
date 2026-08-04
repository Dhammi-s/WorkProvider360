/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Sends transactional email over SMTP using settings from configuration. Every
/// send attempt is recorded to the tenant email log (Sent / Failed).
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly IEmailLogRepository _emailLog;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> options, IEmailLogRepository emailLog, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _emailLog = emailLog;
        _logger = logger;
    }

    public Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default)
        => SendCoreAsync(toAddress, subject, htmlBody, null, null, ct);

    /// <summary>Sends an email with a PDF attachment (e.g. an invoice).</summary>
    public Task SendInvoiceAsync(string toAddress, string subject, string htmlBody, byte[] pdfBytes, string pdfFileName, CancellationToken ct = default)
        => SendCoreAsync(toAddress, subject, htmlBody, pdfBytes, pdfFileName, ct);

    private async Task SendCoreAsync(string toAddress, string subject, string htmlBody, byte[]? attachmentBytes, string? attachmentName, CancellationToken ct)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toAddress);

        Attachment? attachment = null;
        if (attachmentBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(attachmentName))
        {
            attachment = new Attachment(new MemoryStream(attachmentBytes), attachmentName, "application/pdf");
            message.Attachments.Add(attachment);
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
        };

        try
        {
            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {ToAddress} with subject {Subject}", toAddress, subject);
            await RecordAsync(toAddress, subject, htmlBody, "Sent", null, ct);
        }
        catch (Exception ex)
        {
            // Do not surface SMTP failures to the caller of forgot-password flows.
            _logger.LogError(ex, "Failed to send email to {ToAddress}", toAddress);
            await RecordAsync(toAddress, subject, htmlBody, "Failed", ex.Message, ct);
            throw;
        }
        finally
        {
            attachment?.Dispose();
        }
    }

    /// <summary>Best-effort log write; never breaks the email flow.</summary>
    private async Task RecordAsync(string to, string subject, string body, string status, string? error, CancellationToken ct)
    {
        try
        {
            await _emailLog.CreateAsync(new EmailLog
            {
                ToAddress = to,
                Subject = subject,
                Body = body,
                Status = status,
                ErrorMessage = error,
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write email log for {ToAddress}", to);
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

    public Task SendAccountUnlockedAsync(string toAddress, string fullName, string email, string unlockedBy, string temporaryPassword, string loginUrl, CancellationToken ct = default)
    {
        var body = $"""
            <div style="margin:0;padding:0;background:#f1f5f9;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9;padding:24px 0;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;">
                <tr><td align="center">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border-radius:14px;overflow:hidden;border:1px solid #e2e8f0;">
                    <tr>
                      <td style="background:#0f766e;padding:28px 32px;">
                        <div style="display:inline-block;width:40px;height:40px;border-radius:10px;background:rgba(255,255,255,0.14);text-align:center;line-height:40px;font-size:20px;">🔓</div>
                        <div style="color:#ffffff;font-size:18px;font-weight:700;margin-top:14px;">Your account has been unlocked</div>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:28px 32px;color:#334155;font-size:14px;line-height:1.6;">
                        <p style="margin:0 0 14px;">Hi {WebUtility.HtmlEncode(fullName)},</p>
                        <p style="margin:0 0 14px;">Good news — your account was locked after several failed sign-in attempts and has now been
                          <strong style="color:#0f766e;">unlocked by {WebUtility.HtmlEncode(unlockedBy)}</strong>. You can sign in again with the temporary password below.</p>
                        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:18px 0;background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;">
                          <tr><td style="padding:16px 18px;">
                            <div style="font-size:11px;text-transform:uppercase;letter-spacing:.06em;color:#94a3b8;">Email</div>
                            <div style="font-size:14px;font-weight:600;color:#0f172a;margin:2px 0 12px;">{WebUtility.HtmlEncode(email)}</div>
                            <div style="font-size:11px;text-transform:uppercase;letter-spacing:.06em;color:#94a3b8;">Temporary password</div>
                            <div style="font-size:16px;font-weight:700;color:#0f172a;font-family:Consolas,Menlo,monospace;margin-top:2px;">{WebUtility.HtmlEncode(temporaryPassword)}</div>
                          </td></tr>
                        </table>
                        <a href="{loginUrl}" style="display:inline-block;background:#0f766e;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 22px;border-radius:10px;">Sign in now</a>
                        <p style="margin:18px 0 0;color:#64748b;font-size:13px;">For your security, please change this password from your profile right after signing in.</p>
                      </td>
                    </tr>
                    <tr>
                      <td style="padding:16px 32px;background:#f8fafc;border-top:1px solid #e2e8f0;color:#94a3b8;font-size:12px;">
                        If you didn't expect this, please contact your administrator.
                      </td>
                    </tr>
                  </table>
                </td></tr>
              </table>
            </div>
            """;

        return SendAsync(toAddress, "Your account has been unlocked", body, ct);
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
