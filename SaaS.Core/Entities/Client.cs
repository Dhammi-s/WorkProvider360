/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A client (the person whose home is visited). When portal access is enabled a
/// UserId links to a Users row with the Client role.
/// </summary>
public sealed class Client
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? PortalEmail { get; set; }
    public bool? PortalIsActive { get; set; }
    public bool? PortalIsLockedOut { get; set; }
    public string? ServiceTypeNames { get; set; }
}
