/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Text.Json;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Per-user keyboard shortcuts. Any authenticated user (every role) manages
/// their own set; the action key is resolved to a route on the client.
/// </summary>
public sealed class ShortcutService : IShortcutService
{
    private readonly IUserShortcutRepository _shortcuts;

    public ShortcutService(IUserShortcutRepository shortcuts) => _shortcuts = shortcuts;

    public async Task<IReadOnlyList<UserShortcutDto>> GetAsync(int userId, CancellationToken ct = default)
    {
        var rows = await _shortcuts.GetByUserAsync(userId, ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<UserShortcutDto>> ReplaceAsync(int userId, IReadOnlyList<UserShortcutDto> shortcuts, CancellationToken ct = default)
    {
        var clean = (shortcuts ?? new List<UserShortcutDto>())
            .Where(s => !string.IsNullOrWhiteSpace(s.KeyCombo) && !string.IsNullOrWhiteSpace(s.ActionKey))
            .Select(s => new { KeyCombo = s.KeyCombo.Trim(), ActionKey = s.ActionKey.Trim() });

        var saved = await _shortcuts.ReplaceAsync(userId, JsonSerializer.Serialize(clean), ct);
        return saved.Select(Map).ToList();
    }

    private static UserShortcutDto Map(UserShortcut s) => new()
    {
        KeyCombo = s.KeyCombo,
        ActionKey = s.ActionKey,
    };
}
