/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>A single keyboard shortcut (combo + action). Used inbound and outbound.</summary>
public sealed class UserShortcutDto
{
    public string KeyCombo { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty;
}
