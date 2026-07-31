/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>A record of one outbound email attempt (tenant-scoped).</summary>
public sealed class EmailLog
{
    public Guid EmailLogId { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = string.Empty; // Sent / Failed
    public string? ErrorMessage { get; set; }
    public DateTime CreatedOn { get; set; }
}
