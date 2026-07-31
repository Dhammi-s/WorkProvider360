/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>The assigned user's response to a schedule: accept or reject.</summary>
public sealed class RespondScheduleRequestDto
{
    /// <summary>"Accept" or "Reject".</summary>
    [Required]
    public string Action { get; set; } = string.Empty;

    /// <summary>Reason, required when rejecting.</summary>
    public string? Reason { get; set; }
}
