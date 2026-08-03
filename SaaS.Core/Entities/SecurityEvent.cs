/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>A tenant-scoped security audit record (login attempt or detected attack).</summary>
public sealed class SecurityEvent
{
    public Guid SecurityEventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Path { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedOn { get; set; }
}
