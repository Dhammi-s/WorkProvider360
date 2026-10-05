/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>
/// One event on a shift's care log: a clock-in/out, a note or injury report, or
/// a captured client signature (with its image). Ordered chronologically.
/// </summary>
public sealed class CareLogEntryDto
{
    /// <summary>ClockIn | ClockOut | Note | Injury | Signature.</summary>
    public string Type { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Source { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Phase { get; set; }
    public string? SignedByName { get; set; }
    public string? SignatureBase64 { get; set; }
}
