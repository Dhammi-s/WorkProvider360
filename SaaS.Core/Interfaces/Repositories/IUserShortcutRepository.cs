/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IUserShortcutRepository
{
    Task<IReadOnlyList<UserShortcut>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserShortcut>> ReplaceAsync(int userId, string shortcutsJson, CancellationToken ct = default);
}
