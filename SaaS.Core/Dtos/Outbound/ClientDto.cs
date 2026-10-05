/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>List-row / summary view of a client.</summary>
public class ClientDto
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? AccessInstructions { get; set; }
    public string? CareNotes { get; set; }
    public string? Allergies { get; set; }
    public string? MobilityNotes { get; set; }
    public bool PortalEnabled { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? StartDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public int? PortalUserId { get; set; }
    public string? PortalEmail { get; set; }
    public bool PortalIsLockedOut { get; set; }
    public IReadOnlyList<ServiceTypeDto> ServiceTypes { get; set; } = new List<ServiceTypeDto>();
}

/// <summary>Client detail (same shape today; kept distinct for future expansion).</summary>
public sealed class ClientDetailDto : ClientDto
{
}

/// <summary>Capability flags for the Clients / Service Types area, per the caller's role.</summary>
public sealed class ClientAccessDto
{
    public string RoleName { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = "None";
    public bool IsSuperAdmin { get; set; }
    public bool CanViewAll { get; set; }
    public bool CanManage { get; set; }
    public bool CanManageServiceTypes { get; set; }
    public bool CanManageSettings { get; set; }
}

/// <summary>A staff member eligible (or not) for a client visit, with hint flags.</summary>
public sealed class EligibleCaregiverDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public bool IsSameOffice { get; set; }
    public bool HasSkill { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = new List<string>();
}
