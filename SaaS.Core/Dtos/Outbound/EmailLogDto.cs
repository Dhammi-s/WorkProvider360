/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

public sealed class EmailLogDto
{
    public Guid EmailLogId { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>Whether the current user may view logs (and manage the toggles).</summary>
public sealed class LogAccessDto
{
    public bool CanView { get; set; }
    public bool CanManageAccess { get; set; }
}

public sealed class LogSettingsDto
{
    public bool AdminCanViewLogs { get; set; }
    public bool ManagerCanViewLogs { get; set; }
    public DateTime UpdatedOn { get; set; }
}
