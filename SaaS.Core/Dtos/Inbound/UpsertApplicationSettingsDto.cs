/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Inbound;

public sealed class UpsertApplicationSettingsDto
{
    public bool RequirePhone { get; set; }
    public bool RequireAddress { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public string? NotificationEmail { get; set; }
}
