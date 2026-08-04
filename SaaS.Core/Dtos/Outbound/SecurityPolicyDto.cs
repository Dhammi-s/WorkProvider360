/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-04
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>
/// Tenant security policy surfaced to authenticated staff (Team page). Currently
/// carries whether Admins/Managers may unlock locked accounts.
/// </summary>
public sealed class SecurityPolicyDto
{
    public bool AllowStaffUnlock { get; set; }
}
