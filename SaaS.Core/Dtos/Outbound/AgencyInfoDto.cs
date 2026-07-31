/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>Safe, public projection of the current tenant (no connection secrets).</summary>
public sealed class AgencyInfoDto
{
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string? Location { get; set; }
}
