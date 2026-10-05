/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A client signature captured on the worker device at clock-in / clock-out.
/// Kept out of TimeEntries so list queries never load the image blob.
/// </summary>
public sealed class TimeEntrySignature
{
    public int SignatureId { get; set; }
    public int TimeEntryId { get; set; }
    public string Phase { get; set; } = "ClockOut";
    public string SignatureBase64 { get; set; } = string.Empty;
    public string? SignedByName { get; set; }
    public DateTime SignedOnUtc { get; set; }
}
