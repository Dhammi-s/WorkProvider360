/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Optional payload when approving an application. A SuperAdmin supplies the
/// office to place the new user in; an Admin approver's own office is used
/// automatically (this value is ignored for Admins).
/// </summary>
public sealed class ApproveApplicationRequestDto
{
    public Guid? OfficeId { get; set; }
}
