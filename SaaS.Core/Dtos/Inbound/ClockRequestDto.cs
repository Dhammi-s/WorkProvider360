/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Optional body for clock-in / clock-out. GPS is captured when the tenant
/// enables it; a signature is required only for client visits when the matching
/// ClientSettings flag is on.
/// </summary>
public sealed class ClockRequestDto
{
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>Base64 PNG (no data-URI prefix) of the client's signature.</summary>
    public string? SignatureBase64 { get; set; }

    public string? SignedByName { get; set; }
}
