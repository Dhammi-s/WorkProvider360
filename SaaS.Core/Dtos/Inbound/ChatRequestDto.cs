/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>One turn of the assistant conversation.</summary>
public sealed class ChatTurnDto
{
    /// <summary>"user" or "assistant".</summary>
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
}

/// <summary>A question for the in-app assistant plus a short recent history.</summary>
public sealed class ChatRequestDto
{
    [Required, MaxLength(2000)]
    public string Question { get; set; } = string.Empty;

    /// <summary>Recent prior turns (most-recent last), for follow-up context.</summary>
    public List<ChatTurnDto> History { get; set; } = new();
}
