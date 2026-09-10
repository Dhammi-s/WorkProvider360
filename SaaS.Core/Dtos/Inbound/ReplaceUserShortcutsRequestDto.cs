/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Replaces the current user's whole shortcut set.</summary>
public sealed class ReplaceUserShortcutsRequestDto
{
    public List<UserShortcutDto> Shortcuts { get; set; } = new();
}
