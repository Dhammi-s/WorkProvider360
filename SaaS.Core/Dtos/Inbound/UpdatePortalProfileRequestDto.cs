/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Fields a client may edit about themselves from the portal.</summary>
public sealed class UpdatePortalProfileRequestDto
{
    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string? AlternatePhone { get; set; }

    [MaxLength(1000)]
    public string? AccessInstructions { get; set; }

    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(50)]
    public string? EmergencyContactRelation { get; set; }

    [MaxLength(50)]
    public string? PreferredLanguage { get; set; }
}
