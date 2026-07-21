namespace SaaS.Core.Entities;

/// <summary>
/// Single-row (SettingsId = 1) configuration for the public role-application
/// form, held in each tenant database.
/// </summary>
public sealed class ApplicationSettings
{
    public int SettingsId { get; set; }
    public bool RequirePhone { get; set; }
    public bool RequireAddress { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string? NotificationEmail { get; set; }
    public DateTime UpdatedOn { get; set; }
}
