/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Manually add or correct a worked-time record (UTC times).</summary>
public sealed class ManualTimeEntryRequestDto
{
    [Required]
    public DateTime ClockInUtc { get; set; }

    [Required]
    public DateTime ClockOutUtc { get; set; }

    public string? Note { get; set; }
}
