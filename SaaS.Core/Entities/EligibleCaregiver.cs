/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>A staff user considered for a client visit, with eligibility flags.</summary>
public sealed class EligibleCaregiver
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public bool IsSameOffice { get; set; }
    public bool HasSkill { get; set; }
    public string? SkillNames { get; set; }
}
