/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>An in-app notification for a user (tenant-scoped).</summary>
public sealed class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedOn { get; set; }
}
