using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Scheduling: schedules, assigned-user actions (accept/reject/injury/notes),
/// time tracking, reports and access settings. Fine-grained permission is
/// enforced in the service layer from the caller's role + tenant settings,
/// so the class only requires an authenticated user.
/// </summary>
[Authorize]
public sealed class SchedulingController : BaseApiController
{
    private readonly ISchedulingService _scheduling;

    public SchedulingController(ISchedulingService scheduling) => _scheduling = scheduling;

    // ----------------------------------------------------------- Access/settings

    /// <summary>The current user's effective scheduling permissions.</summary>
    [HttpGet("access")]
    public async Task<ActionResult<ApiResponse<SchedulingAccessDto>>> GetAccess(CancellationToken ct)
    {
        var access = await _scheduling.GetAccessAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<SchedulingAccessDto>.Ok(access));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<SchedulingSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _scheduling.GetSettingsAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<SchedulingSettingsDto>.Ok(settings));
    }

    /// <summary>SuperAdmin-only: set Admin/Manager access levels (None/Read/Write).</summary>
    [HttpPut("settings/access")]
    public async Task<ActionResult<ApiResponse<SchedulingSettingsDto>>> UpdateAccess(
        [FromBody] UpdateSchedulingAccessDto request, CancellationToken ct)
    {
        var settings = await _scheduling.UpdateAccessAsync(request, CurrentRoleId, ct);
        return Ok(ApiResponse<SchedulingSettingsDto>.Ok(settings, "Access settings saved."));
    }

    [HttpPut("settings/defaults")]
    public async Task<ActionResult<ApiResponse<SchedulingSettingsDto>>> UpdateDefaults(
        [FromBody] UpdateSchedulingDefaultsDto request, CancellationToken ct)
    {
        var settings = await _scheduling.UpdateDefaultsAsync(request, CurrentRoleId, ct);
        return Ok(ApiResponse<SchedulingSettingsDto>.Ok(settings, "Defaults saved."));
    }

    /// <summary>Active users a manager can assign schedules to.</summary>
    [HttpGet("assignable-users")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAssignableUsers(CancellationToken ct)
    {
        var users = await _scheduling.GetAssignableUsersAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    // ------------------------------------------------------------------ Schedules

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ScheduleDto>>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? userId, CancellationToken ct)
    {
        var items = await _scheduling.GetSchedulesAsync(from, to, userId, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<ScheduleDto>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ScheduleDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var detail = await _scheduling.GetScheduleAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleDetailDto>.Ok(detail));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Create(
        [FromBody] CreateScheduleRequestDto request, CancellationToken ct)
    {
        var created = await _scheduling.CreateAsync(request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleDto>.Ok(created, "Schedule created."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Update(
        int id, [FromBody] UpdateScheduleRequestDto request, CancellationToken ct)
    {
        var updated = await _scheduling.UpdateAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleDto>.Ok(updated, "Schedule updated."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(int id, CancellationToken ct)
    {
        await _scheduling.DeleteAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("Schedule deleted."));
    }

    // ---------------------------------------------------------- Assigned-user actions

    [HttpPost("{id:int}/respond")]
    public async Task<ActionResult<ApiResponse<ScheduleDto>>> Respond(
        int id, [FromBody] RespondScheduleRequestDto request, CancellationToken ct)
    {
        var updated = await _scheduling.RespondAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleDto>.Ok(updated, $"Schedule {updated.Status.ToLowerInvariant()}."));
    }

    [HttpGet("{id:int}/notes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ScheduleNoteDto>>>> GetNotes(int id, CancellationToken ct)
    {
        var notes = await _scheduling.GetNotesAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<ScheduleNoteDto>>.Ok(notes));
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<ApiResponse<ScheduleNoteDto>>> AddNote(
        int id, [FromBody] CreateScheduleNoteRequestDto request, CancellationToken ct)
    {
        var note = await _scheduling.AddNoteAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleNoteDto>.Ok(note, "Note added."));
    }

    // ------------------------------------------------------------------ Time tracking

    [HttpPost("{id:int}/time/clock-in")]
    public async Task<ActionResult<ApiResponse<object?>>> ClockIn(int id, CancellationToken ct)
    {
        await _scheduling.ClockInAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("Clocked in."));
    }

    [HttpPost("{id:int}/time/clock-out")]
    public async Task<ActionResult<ApiResponse<object?>>> ClockOut(int id, CancellationToken ct)
    {
        await _scheduling.ClockOutAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("Clocked out."));
    }

    [HttpGet("{id:int}/time")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TimeEntryDto>>>> GetTime(int id, CancellationToken ct)
    {
        var entries = await _scheduling.GetTimeEntriesAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<TimeEntryDto>>.Ok(entries));
    }

    [HttpPost("{id:int}/time")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> AddTime(
        int id, [FromBody] ManualTimeEntryRequestDto request, CancellationToken ct)
    {
        var entry = await _scheduling.AddManualTimeAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<TimeEntryDto>.Ok(entry, "Time entry added."));
    }

    [HttpPut("{id:int}/time/{entryId:int}")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> UpdateTime(
        int id, int entryId, [FromBody] ManualTimeEntryRequestDto request, CancellationToken ct)
    {
        var entry = await _scheduling.UpdateTimeAsync(id, entryId, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<TimeEntryDto>.Ok(entry, "Time entry updated."));
    }

    // ------------------------------------------------------------- Live location

    /// <summary>Assigned user posts a GPS position (only while clocked in).</summary>
    [HttpPost("{id:int}/location")]
    public async Task<ActionResult<ApiResponse<object?>>> RecordLocation(
        int id, [FromBody] RecordLocationRequestDto request, CancellationToken ct)
    {
        await _scheduling.RecordLocationAsync(id, request, CurrentUserId, CurrentRoleId, CurrentAgencyId, ct);
        return Ok(ApiResponse.Ok("Location recorded."));
    }

    /// <summary>Breadcrumb trail of positions recorded for a schedule.</summary>
    [HttpGet("{id:int}/location/trail")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LocationPingDto>>>> GetTrail(int id, CancellationToken ct)
    {
        var trail = await _scheduling.GetTrailAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<LocationPingDto>>.Ok(trail));
    }

    /// <summary>Latest position of every currently-active (clocked-in) schedule.</summary>
    [HttpGet("location/live")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LiveLocationDto>>>> GetLive(CancellationToken ct)
    {
        var live = await _scheduling.GetLiveLocationsAsync(CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<LiveLocationDto>>.Ok(live));
    }

    // ---------------------------------------------------------------------- Reports

    [HttpGet("reports")]
    public async Task<ActionResult<ApiResponse<ScheduleReportDto>>> GetReport(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? userId, CancellationToken ct)
    {
        var report = await _scheduling.GetReportAsync(from, to, userId, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ScheduleReportDto>.Ok(report));
    }
}
