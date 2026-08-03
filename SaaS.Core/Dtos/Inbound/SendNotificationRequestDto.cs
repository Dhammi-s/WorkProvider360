/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>An admin sends an in-app notification to a user.</summary>
public sealed class SendNotificationRequestDto
{
    [Required]
    public int UserId { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
