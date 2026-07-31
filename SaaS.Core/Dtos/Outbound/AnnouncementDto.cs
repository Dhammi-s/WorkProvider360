/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
