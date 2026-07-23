namespace SaaS.Core.Dtos.Outbound;

public sealed class AnnouncementDto
{
    public Guid AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>What the current user can do with announcements.</summary>
public sealed class AnnouncementViewDto
{
    public bool CanView { get; set; }
    public bool CanManage { get; set; }
    public IReadOnlyList<AnnouncementDto> Announcements { get; set; } = new List<AnnouncementDto>();
}

public sealed class AnnouncementSettingsDto
{
    public bool ShowToAdmin { get; set; }
    public bool ShowToManager { get; set; }
    public bool ShowToUser { get; set; }
    public DateTime UpdatedOn { get; set; }
}
