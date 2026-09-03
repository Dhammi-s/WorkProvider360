/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Create a client. Conditional required rules are enforced in the service from ClientSettings.</summary>
public class CreateClientRequestDto
{
    public Guid? OfficeId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? AlternatePhone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [Required, MaxLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(50)]
    public string? EmergencyContactRelation { get; set; }

    [MaxLength(50)]
    public string? PreferredLanguage { get; set; }

    [MaxLength(1000)]
    public string? AccessInstructions { get; set; }

    public string? CareNotes { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    [MaxLength(500)]
    public string? MobilityNotes { get; set; }

    public DateTime? StartDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public List<int> ServiceTypeIds { get; set; } = new();
}

/// <summary>Edit an existing client (same fields as create; status/portal handled separately).</summary>
public sealed class UpdateClientRequestDto : CreateClientRequestDto
{
}

public sealed class UpdateClientStatusRequestDto
{
    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Active | OnHold | Inactive
}

public sealed class SetClientPortalAccessRequestDto
{
    public bool Enabled { get; set; }

    /// <summary>Send (or resend) the credentials email when enabling. Defaults to the tenant setting.</summary>
    public bool? SendEmail { get; set; }
}
