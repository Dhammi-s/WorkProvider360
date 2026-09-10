/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>A user-defined keyboard shortcut: a key combo mapped to an action key.</summary>
public sealed class UserShortcut
{
    public int ShortcutId { get; set; }
    public int UserId { get; set; }
    public string KeyCombo { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
