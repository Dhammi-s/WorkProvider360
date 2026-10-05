/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>A staff member's editable profile plus their skills and availability.</summary>
public sealed class UserProfileDto
{
    public int UserId { get; set; }
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
    public IReadOnlyList<ServiceTypeDto> Skills { get; set; } = new List<ServiceTypeDto>();
    public IReadOnlyList<AvailabilitySlotDto> Availability { get; set; } = new List<AvailabilitySlotDto>();
}

/// <summary>
/// One weekly availability range. DayOfWeek is 0 (Sunday) - 6 (Saturday);
/// times are "HH:mm" strings. Used both inbound and outbound.
/// </summary>
public sealed class AvailabilitySlotDto
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
