namespace SaaS.Core.Entities;

public sealed class Announcement
{
    public Guid AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>Single-row (SettingsId = 1) config for which roles see announcements.</summary>
public sealed class AnnouncementSettings
{
    public int SettingsId { get; set; }
    public bool ShowToAdmin { get; set; }
    public bool ShowToManager { get; set; }
    public bool ShowToUser { get; set; }
    public DateTime UpdatedOn { get; set; }
}
