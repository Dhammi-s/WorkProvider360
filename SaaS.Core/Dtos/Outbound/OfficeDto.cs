/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

public sealed class OfficeDto
{
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public Guid? TimezoneId { get; set; }
    public string? TimezoneName { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedOn { get; set; }
}

public sealed class OfficeMemberDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
