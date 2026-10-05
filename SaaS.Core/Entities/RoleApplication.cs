/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A submitted request for elevated (Admin/Manager) access, awaiting review.
/// </summary>
public sealed class RoleApplication
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Qualifications { get; set; }
    public decimal? YearsOfExperience { get; set; }
    public string? About { get; set; }
    public bool? HasDrivingLicense { get; set; }
    public bool? HasVehicle { get; set; }
    public int RequestedRoleId { get; set; }
    public string? RequestedRoleName { get; set; }
    public decimal? DesiredSalary { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedOn { get; set; }
    public DateTime CreatedOn { get; set; }
}
