namespace SaaS.Core.Dtos.Outbound;

public sealed class ApplicationSettingsDto
{
    public bool RequirePhone { get; set; }
    public bool RequireAddress { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string? NotificationEmail { get; set; }
    public DateTime UpdatedOn { get; set; }
}
