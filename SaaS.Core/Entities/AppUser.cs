/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A user row from a tenant database "Users" table. The password is stored as a
/// salted SHA-512 hash; the salt is stored alongside it.
/// </summary>
public sealed class AppUser
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; }
    public int AccessFailedCount { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
