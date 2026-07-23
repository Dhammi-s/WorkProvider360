using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Announcements. Any authenticated user can read those visible to their role;
/// only a SuperAdmin can create/deactivate and set role visibility.
/// </summary>
[Authorize]
public sealed class AnnouncementsController : BaseApiController
{
    private readonly IAnnouncementService _announcements;

    public AnnouncementsController(IAnnouncementService announcements) => _announcements = announcements;

    /// <summary>Announcements visible to the current user + can-manage flag.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AnnouncementViewDto>>> Get(CancellationToken ct)
    {
        var view = await _announcements.GetForViewerAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<AnnouncementViewDto>.Ok(view));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AnnouncementDto>>> Create(
        [FromBody] CreateAnnouncementRequestDto request, CancellationToken ct)
    {
        var created = await _announcements.CreateAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse<AnnouncementDto>.Ok(created, "Announcement published."));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Deactivate(Guid id, CancellationToken ct)
    {
        await _announcements.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Announcement removed."));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<AnnouncementSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _announcements.GetSettingsAsync(ct);
        return Ok(ApiResponse<AnnouncementSettingsDto>.Ok(settings));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<AnnouncementSettingsDto>>> UpdateSettings(
        [FromBody] UpdateAnnouncementSettingsDto request, CancellationToken ct)
    {
        var settings = await _announcements.UpdateSettingsAsync(request, ct);
        return Ok(ApiResponse<AnnouncementSettingsDto>.Ok(settings, "Announcement visibility updated."));
    }
}
