using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Email logs. Visibility is resolved in the service: SuperAdmin always; Admins /
/// Managers only when the SuperAdmin has toggled it on. Managing the toggles is
/// SuperAdmin-only.
/// </summary>
[Authorize]
public sealed class LogsController : BaseApiController
{
    private readonly ILogService _logs;

    public LogsController(ILogService logs) => _logs = logs;

    /// <summary>Whether the current user may view logs / manage the toggles.</summary>
    [HttpGet("access")]
    public async Task<ActionResult<ApiResponse<LogAccessDto>>> Access(CancellationToken ct)
    {
        var access = await _logs.GetAccessAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<LogAccessDto>.Ok(access));
    }

    [HttpGet("emails")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EmailLogDto>>>> Emails(
        [FromQuery] int top = 200, CancellationToken ct = default)
    {
        var logs = await _logs.GetEmailLogsAsync(CurrentRoleId, top, ct);
        return Ok(ApiResponse<IReadOnlyList<EmailLogDto>>.Ok(logs));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<LogSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _logs.GetSettingsAsync(ct);
        return Ok(ApiResponse<LogSettingsDto>.Ok(settings));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<LogSettingsDto>>> UpdateSettings(
        [FromBody] UpdateLogSettingsDto request, CancellationToken ct)
    {
        var settings = await _logs.UpdateSettingsAsync(request, ct);
        return Ok(ApiResponse<LogSettingsDto>.Ok(settings, "Log access updated."));
    }
}
