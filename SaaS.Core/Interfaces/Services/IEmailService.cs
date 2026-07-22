namespace SaaS.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);

    Task SendPasswordResetAsync(string toAddress, string fullName, string resetLink, CancellationToken ct = default);

    /// <summary>Confirmation to the applicant that their application was received.</summary>
    Task SendApplicationReceivedAsync(string toAddress, string fullName, string roleName, CancellationToken ct = default);

    /// <summary>Notifies a reviewer (admin) that a new application arrived.</summary>
    Task SendApplicationNotificationAsync(string toAddress, string applicantName, string applicantEmail, string roleName, CancellationToken ct = default);

    /// <summary>Sends login credentials to an approved applicant.</summary>
    Task SendCredentialsAsync(string toAddress, string fullName, string email, string temporaryPassword, string loginUrl, CancellationToken ct = default);

    /// <summary>Notifies an applicant that their application was rejected.</summary>
    Task SendApplicationRejectedAsync(string toAddress, string fullName, string roleName, string reason, CancellationToken ct = default);

    /// <summary>Notifies the assigned user that a schedule was assigned to them.</summary>
    Task SendScheduleAssignedAsync(string toAddress, string userName, string title, string? location, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

    /// <summary>Notifies an admin/manager that a schedule was created for a user.</summary>
    Task SendScheduleNotificationAsync(string toAddress, string title, string assignedUserName, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

    /// <summary>Alerts an admin/manager that a user reported an injury on a schedule.</summary>
    Task SendScheduleInjuryReportAsync(string toAddress, string title, string reporterName, string message, CancellationToken ct = default);
}
