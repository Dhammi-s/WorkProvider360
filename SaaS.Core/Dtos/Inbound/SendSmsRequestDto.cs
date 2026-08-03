/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Manual SMS send. Provide either a <see cref="UserId"/> (uses that user's
/// stored phone) or an explicit <see cref="ToNumber"/>.
/// </summary>
public sealed class SendSmsRequestDto
{
    /// <summary>Existing user to text; their stored phone number is used.</summary>
    public int? UserId { get; set; }

    /// <summary>Explicit destination number (used when no UserId, or the user has no phone on file).</summary>
    [Phone]
    public string? ToNumber { get; set; }

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
