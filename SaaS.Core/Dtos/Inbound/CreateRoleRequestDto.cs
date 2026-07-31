/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Payload for creating a role. Because role ids are static and shared across
/// tenants, the id is supplied explicitly (not auto-generated) so it stays
/// consistent everywhere.
/// </summary>
public sealed class CreateRoleRequestDto
{
    [Required, Range(1, int.MaxValue)]
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
