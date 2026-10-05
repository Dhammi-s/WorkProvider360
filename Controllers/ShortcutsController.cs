/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// The current user's keyboard shortcuts. Available to every role (staff and
/// portal clients); each user only ever reads or writes their own set.
/// </summary>
[Authorize]
public sealed class ShortcutsController : BaseApiController
{
    private readonly IShortcutService _shortcuts;

    public ShortcutsController(IShortcutService shortcuts) => _shortcuts = shortcuts;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserShortcutDto>>>> Get(CancellationToken ct)
    {
        var items = await _shortcuts.GetAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<UserShortcutDto>>.Ok(items));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserShortcutDto>>>> Replace(
        [FromBody] ReplaceUserShortcutsRequestDto request, CancellationToken ct)
    {
        var items = await _shortcuts.ReplaceAsync(CurrentUserId, request.Shortcuts, ct);
        return Ok(ApiResponse<IReadOnlyList<UserShortcutDto>>.Ok(items, "Shortcuts saved."));
    }
}
