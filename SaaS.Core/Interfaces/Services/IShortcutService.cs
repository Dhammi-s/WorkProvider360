/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>Per-user keyboard shortcuts, available to every role.</summary>
public interface IShortcutService
{
    Task<IReadOnlyList<UserShortcutDto>> GetAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserShortcutDto>> ReplaceAsync(int userId, IReadOnlyList<UserShortcutDto> shortcuts, CancellationToken ct = default);
}
