/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Create/update a staff member's profile, skills and weekly availability.</summary>
public sealed class UpsertUserProfileRequestDto
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Qualifications { get; set; }
    public decimal? YearsOfExperience { get; set; }
    public string? About { get; set; }
    public bool HasDrivingLicense { get; set; }
    public bool HasVehicle { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? HireDate { get; set; }
    public List<int> ServiceTypeIds { get; set; } = new();
    public List<AvailabilitySlotDto> Availability { get; set; } = new();
}
