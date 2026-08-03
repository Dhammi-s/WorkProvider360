/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>A single persisted assistant-chat message for a user (tenant-scoped).</summary>
public sealed class ChatMessage
{
    public int ChatMessageId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "user"; // 'user' | 'assistant'
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
