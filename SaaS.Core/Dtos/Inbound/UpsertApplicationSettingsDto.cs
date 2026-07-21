namespace SaaS.Core.Dtos.Inbound;

public sealed class UpsertApplicationSettingsDto
{
    public bool RequirePhone { get; set; }
    public bool RequireAddress { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string? NotificationEmail { get; set; }
}
