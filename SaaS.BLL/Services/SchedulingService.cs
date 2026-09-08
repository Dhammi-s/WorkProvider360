/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.Extensions.Logging;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Scheduling workflow. Access is role-based: SuperAdmin always has full write;
/// Admin/Manager get None/Read/Write from <see cref="SchedulingSettings"/>; a
/// regular User sees only their own schedules and can accept/reject, add notes,
/// report injuries and track time on them. Every method enforces this.
/// </summary>
public sealed class SchedulingService : ISchedulingService
{
    private const string None = "None";
    private const string Read = "Read";
    private const string Write = "Write";
    private const string Self = "Self";

    private readonly IScheduleRepository _schedules;
    private readonly ISchedulingSettingsRepository _settings;
    private readonly ILocationRepository _locations;
    private readonly ILocationBroadcaster _broadcaster;
    private readonly IUserService _users;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly ILogger<SchedulingService> _logger;
    private readonly IClientRepository _clients;
    private readonly IClientSettingsRepository _clientSettings;

    public SchedulingService(
        IScheduleRepository schedules,
        ISchedulingSettingsRepository settings,
        ILocationRepository locations,
        ILocationBroadcaster broadcaster,
        IUserService users,
        IEmailService email,
        ISmsService sms,
        IClientRepository clients,
        IClientSettingsRepository clientSettings,
        ILogger<SchedulingService> logger)
    {
        _schedules = schedules;
        _settings = settings;
        _locations = locations;
        _broadcaster = broadcaster;
        _users = users;
        _email = email;
        _sms = sms;
        _logger = logger;
        _clients = clients;
        _clientSettings = clientSettings;
    }

    // -------------------------------------------------------- Access / settings

    public async Task<SchedulingAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        return BuildAccess(roleId, level);
    }

    public async Task<SchedulingSettingsDto> GetSettingsAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        // Anyone who can at least read the scheduler may read the settings/defaults.
        var level = LevelForRole(roleId, settings);
        if (level == None)
            throw AppException.Forbidden("You do not have access to scheduling settings.");
        return MapSettings(settings);
    }

    public async Task<SchedulingSettingsDto> UpdateAccessAsync(UpdateSchedulingAccessDto request, int roleId, CancellationToken ct = default)
    {
        // SuperAdmin sets both Admin and Manager access. Admin outranks Manager,
        // so an Admin may set the Manager level only (their own level is preserved).
        if (roleId == RoleConstants.SuperAdminId)
        {
            var savedBySuper = await _settings.UpdateAccessAsync(
                CanonLevel(request.AdminAccess), CanonLevel(request.ManagerAccess), ct);
            return MapSettings(savedBySuper);
        }

        if (roleId == RoleConstants.AdminId)
        {
            var current = await _settings.GetAsync(ct);
            var saved = await _settings.UpdateAccessAsync(
                CanonLevel(current?.AdminAccess ?? Write), CanonLevel(request.ManagerAccess), ct);
            return MapSettings(saved);
        }

        throw AppException.Forbidden("You do not have permission to change scheduling access.");
    }

    public async Task<SchedulingSettingsDto> UpdateDefaultsAsync(UpdateSchedulingDefaultsDto request, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);

        var saved = await _settings.UpdateDefaultsAsync(
            request.DefaultPayRatePerHour,
            request.DefaultOvertimeMultiplier <= 0 ? 1.5m : request.DefaultOvertimeMultiplier,
            request.NotifyAdminOnCreate,
            request.NotifyManagerOnCreate,
            request.AutoClockEnabled,
            ct);
        return MapSettings(saved);
    }

    public async Task<IReadOnlyList<UserDto>> GetAssignableUsersAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);

        var users = await _users.GetAllAsync(ct);
        // You can only schedule people who rank strictly below you: Admin -> Manager/User,
        // Manager -> User only, SuperAdmin -> anyone.
        var myRank = RankOfRoleId(roleId);
        return users.Where(u => u.IsActive && u.RoleName != RoleConstants.Client && RankOfRoleName(u.RoleName) > myRank).ToList();
    }

    // --------------------------------------------------------------- Schedules

    public async Task<IReadOnlyList<ScheduleDto>> GetSchedulesAsync(
        DateTime? fromUtc, DateTime? toUtc, int? assignedUserId, int? clientId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        if (level == None)
            throw AppException.Forbidden("You do not have access to the scheduler.");

        await MaybeAutoClockAsync(settings, ct);

        // A regular User only ever sees their own schedules.
        var effectiveUserId = level == Self ? currentUserId : assignedUserId;

        var rows = await _schedules.GetAllAsync(fromUtc, toUtc, effectiveUserId, clientId, ct);
        return rows.Select(MapSchedule).ToList();
    }

    /// <summary>
    /// Overlapping shifts already assigned to the caregiver in the given window
    /// (any client). Display-only — creation is never hard-blocked; the UI shows
    /// these and lets the scheduler save anyway.
    /// </summary>
    public async Task<IReadOnlyList<ScheduleConflictDto>> GetConflictsAsync(
        int assignedUserId, DateTime startUtc, DateTime endUtc, int? excludeScheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);

        if (endUtc <= startUtc || assignedUserId <= 0)
            return new List<ScheduleConflictDto>();

        // Pull the caregiver's shifts around the window (a day of slack each side
        // covers timezone/long-shift edges), then keep true overlaps.
        var rows = await _schedules.GetAllAsync(startUtc.AddDays(-1), endUtc.AddDays(1), assignedUserId, null, ct);

        return rows
            .Where(s => s.ScheduleId != (excludeScheduleId ?? 0)
                     && s.Status != "Cancelled" && s.Status != "Rejected"
                     && s.StartUtc < endUtc && s.EndUtc > startUtc)
            .OrderBy(s => s.StartUtc)
            .Select(s => new ScheduleConflictDto
            {
                ScheduleId = s.ScheduleId,
                Title = s.Title,
                ClientId = s.ClientId,
                ClientName = s.ClientName ?? s.CustomerName,
                AssignedUserId = s.AssignedUserId,
                AssignedUserName = s.AssignedUserName ?? string.Empty,
                StartUtc = s.StartUtc,
                EndUtc = s.EndUtc,
                Status = s.Status,
            })
            .ToList();
    }


    public async Task<ScheduleDetailDto> GetScheduleAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        await MaybeAutoClockAsync(settings, ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var notes = await _schedules.GetNotesAsync(scheduleId, ct);
        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);

        return new ScheduleDetailDto
        {
            Schedule = MapSchedule(schedule),
            Notes = notes.Select(MapNote).ToList(),
            TimeEntries = entries.Select(MapTime).ToList(),
        };
    }

    public async Task<ScheduleDto> CreateAsync(CreateScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);
        ValidateWindow(request.StartUtc, request.EndUtc);

        var assignee = await _users.GetByIdAsync(request.AssignedUserId, ct)
            ?? throw AppException.BadRequest("The selected user does not exist.");
        if (!assignee.IsActive)
            throw AppException.BadRequest("The selected user is not active.");
        EnsureCanScheduleFor(roleId, assignee);

        var (client, customerName, location) = await ResolveClientForScheduleAsync(
            request.ClientId, request.ServiceTypeId, request.CustomerName, request.Location, assignee, ct);

        var id = await _schedules.CreateAsync(new Schedule
        {
            Title = request.Title.Trim(),
            CustomerName = customerName,
            Location = location,
            ClientId = request.ClientId,
            ServiceTypeId = request.ServiceTypeId,
            AssignedUserId = request.AssignedUserId,
            StartUtc = request.StartUtc,
            EndUtc = request.EndUtc,
            PayRatePerHour = request.PayRatePerHour,
            OvertimeMultiplier = request.OvertimeMultiplier <= 0 ? 1.5m : request.OvertimeMultiplier,
            ColorTag = Clean(request.ColorTag),
            CreatedByUserId = currentUserId,
        }, ct);

        var created = await _schedules.GetByIdAsync(id, ct)
            ?? throw AppException.NotFound("Schedule not found after creation.");

        await SendCreationEmailsAsync(created, assignee, request.NotifyAdmin, request.NotifyManager, ct);

        await MaybeNotifyClientAsync(created, client, ct);

        return MapSchedule(created);
    }

    public async Task<ScheduleDto> UpdateAsync(int scheduleId, UpdateScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);
        ValidateWindow(request.StartUtc, request.EndUtc);

        _ = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        var assignee = await _users.GetByIdAsync(request.AssignedUserId, ct)
            ?? throw AppException.BadRequest("The selected user does not exist.");
        EnsureCanScheduleFor(roleId, assignee);

        var (client, customerName, location) = await ResolveClientForScheduleAsync(
            request.ClientId, request.ServiceTypeId, request.CustomerName, request.Location, assignee, ct);

        await _schedules.UpdateAsync(new Schedule
        {
            ScheduleId = scheduleId,
            Title = request.Title.Trim(),
            CustomerName = customerName,
            Location = location,
            ClientId = request.ClientId,
            ServiceTypeId = request.ServiceTypeId,
            AssignedUserId = request.AssignedUserId,
            StartUtc = request.StartUtc,
            EndUtc = request.EndUtc,
            PayRatePerHour = request.PayRatePerHour,
            OvertimeMultiplier = request.OvertimeMultiplier <= 0 ? 1.5m : request.OvertimeMultiplier,
            ColorTag = Clean(request.ColorTag),
        }, ct);

        var updated = await _schedules.GetByIdAsync(scheduleId, ct)!;
        return MapSchedule(updated!);
    }

    public async Task DeleteAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);

        _ = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        await _schedules.DeleteAsync(scheduleId, ct);
    }

    // ------------------------------------------------------ Assigned-user actions

    public async Task<ScheduleDto> RespondAsync(int scheduleId, RespondScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        if (schedule.AssignedUserId != currentUserId)
            throw AppException.Forbidden("Only the assigned user can respond to this schedule.");

        var action = (request.Action ?? string.Empty).Trim();
        if (string.Equals(action, "Accept", StringComparison.OrdinalIgnoreCase))
        {
            await _schedules.UpdateStatusAsync(scheduleId, "Accepted", null, ct);
        }
        else if (string.Equals(action, "Reject", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw AppException.BadRequest("A reason is required to reject a schedule.");
            await _schedules.UpdateStatusAsync(scheduleId, "Rejected", request.Reason.Trim(), ct);
        }
        else
        {
            throw AppException.BadRequest("Action must be 'Accept' or 'Reject'.");
        }

        var updated = await _schedules.GetByIdAsync(scheduleId, ct)!;
        return MapSchedule(updated!);
    }

    public async Task<ScheduleNoteDto> AddNoteAsync(int scheduleId, CreateScheduleNoteRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        var canManage = CanManage(roleId, settings);
        var isAssignee = schedule.AssignedUserId == currentUserId;
        if (!canManage && !isAssignee)
            throw AppException.Forbidden("You cannot add notes to this schedule.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw AppException.BadRequest("A message is required.");

        var noteType = string.Equals(request.NoteType, "Injury", StringComparison.OrdinalIgnoreCase) ? "Injury" : "Note";

        var noteId = await _schedules.CreateNoteAsync(new ScheduleNote
        {
            ScheduleId = scheduleId,
            AuthorUserId = currentUserId,
            NoteType = noteType,
            Message = request.Message.Trim(),
        }, ct);

        var author = await _users.GetByIdAsync(currentUserId, ct);

        if (noteType == "Injury")
            await SendInjuryEmailsAsync(schedule, author?.FullName ?? "A user", request.Message.Trim(), ct);

        return new ScheduleNoteDto
        {
            NoteId = noteId,
            ScheduleId = scheduleId,
            AuthorUserId = currentUserId,
            AuthorName = author?.FullName ?? string.Empty,
            NoteType = noteType,
            Message = request.Message.Trim(),
            CreatedOn = DateTime.UtcNow,
        };
    }

    public async Task<IReadOnlyList<ScheduleNoteDto>> GetNotesAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var notes = await _schedules.GetNotesAsync(scheduleId, ct);
        return notes.Select(MapNote).ToList();
    }

    // ------------------------------------------------------------- Time tracking

    public async Task ClockInAsync(int scheduleId, ClockRequestDto? request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var schedule = await RequireAssignedScheduleAsync(scheduleId, currentUserId, ct);

        var open = await _schedules.GetOpenTimeEntryAsync(scheduleId, currentUserId, ct);
        if (open is not null)
            throw AppException.BadRequest("You are already clocked in for this schedule.");

        // Once a full clock-in/out cycle has been recorded the clock is frozen.
        var existing = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        if (schedule.Status == "Completed" || existing.Any(e => e.ClockOutUtc is not null))
            throw AppException.BadRequest("This shift is already completed and the clock is locked.");

        var signature = await ResolveSignatureAsync(schedule, request, "ClockIn", ct);

        var entryId = await _schedules.ClockInAsync(scheduleId, currentUserId, request?.Latitude, request?.Longitude, ct);
        await SaveSignatureAsync(entryId, "ClockIn", signature, ct);

        // Reflect that work has started (unless already completed/cancelled).
        if (schedule.Status is "Scheduled" or "Accepted")
            await _schedules.UpdateStatusAsync(scheduleId, "InProgress", schedule.RejectionReason, ct);
    }

    public async Task ClockOutAsync(int scheduleId, ClockRequestDto? request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var schedule = await RequireAssignedScheduleAsync(scheduleId, currentUserId, ct);

        var signature = await ResolveSignatureAsync(schedule, request, "ClockOut", ct);

        var closedEntryId = await _schedules.ClockOutAsync(scheduleId, currentUserId, request?.Latitude, request?.Longitude, ct);
        if (closedEntryId == 0)
            throw AppException.BadRequest("You are not currently clocked in for this schedule.");

        await SaveSignatureAsync(closedEntryId, "ClockOut", signature, ct);

        // Clocking out finishes the job.
        await _schedules.UpdateStatusAsync(scheduleId, "Completed", null, ct);
    }

    public async Task<TimeEntryDto> AddManualTimeAsync(int scheduleId, ManualTimeEntryRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        var canManage = CanManage(roleId, settings);
        var isAssignee = schedule.AssignedUserId == currentUserId;
        if (!canManage && !isAssignee)
            throw AppException.Forbidden("You cannot add time to this schedule.");

        ValidateWindow(request.ClockInUtc, request.ClockOutUtc);

        var id = await _schedules.CreateTimeEntryAsync(new TimeEntry
        {
            ScheduleId = scheduleId,
            UserId = schedule.AssignedUserId,
            ClockInUtc = request.ClockInUtc,
            ClockOutUtc = request.ClockOutUtc,
            Note = Clean(request.Note),
        }, ct);

        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        var created = entries.FirstOrDefault(e => e.TimeEntryId == id);
        return created is null ? new TimeEntryDto { TimeEntryId = id, ScheduleId = scheduleId } : MapTime(created);
    }

    public async Task<TimeEntryDto> UpdateTimeAsync(int scheduleId, int timeEntryId, ManualTimeEntryRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        var canManage = CanManage(roleId, settings);
        var isAssignee = schedule.AssignedUserId == currentUserId;
        if (!canManage && !isAssignee)
            throw AppException.Forbidden("You cannot edit time on this schedule.");

        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        if (entries.All(e => e.TimeEntryId != timeEntryId))
            throw AppException.NotFound("Time entry not found.");

        ValidateWindow(request.ClockInUtc, request.ClockOutUtc);

        await _schedules.UpdateTimeEntryAsync(new TimeEntry
        {
            TimeEntryId = timeEntryId,
            ClockInUtc = request.ClockInUtc,
            ClockOutUtc = request.ClockOutUtc,
            Note = Clean(request.Note),
        }, ct);

        var refreshed = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        var updated = refreshed.First(e => e.TimeEntryId == timeEntryId);
        return MapTime(updated);
    }

    public async Task<IReadOnlyList<TimeEntryDto>> GetTimeEntriesAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        await MaybeAutoClockAsync(settings, ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        return entries.Select(MapTime).ToList();
    }

    // ---------------------------------------------------------------- Reporting

    public async Task<ScheduleReportDto> GetReportAsync(
        DateTime fromUtc, DateTime toUtc, int? assignedUserId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        if (toUtc <= fromUtc)
            throw AppException.BadRequest("The report end date must be after the start date.");

        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        if (level == None)
            throw AppException.Forbidden("You do not have access to scheduling reports.");

        await MaybeAutoClockAsync(settings, ct);

        var effectiveUserId = level == Self ? currentUserId : assignedUserId;

        var rows = await _schedules.GetReportAsync(fromUtc, toUtc, effectiveUserId, ct);

        var grouped = rows
            .GroupBy(r => new { r.AssignedUserId, r.AssignedUserName })
            .Select(g =>
            {
                decimal regularHours = 0, overtimeHours = 0, regularPay = 0, overtimePay = 0;
                foreach (var r in g)
                {
                    var worked = (decimal)r.WorkedSeconds / 3600m;
                    var scheduled = (decimal)Math.Max(0, (r.EndUtc - r.StartUtc).TotalHours);
                    var regular = Math.Min(worked, scheduled);
                    var overtime = Math.Max(0, worked - scheduled);
                    var otMult = r.OvertimeMultiplier <= 0 ? 1.5m : r.OvertimeMultiplier;

                    regularHours += regular;
                    overtimeHours += overtime;
                    regularPay += regular * r.PayRatePerHour;
                    overtimePay += overtime * r.PayRatePerHour * otMult;
                }

                return new ScheduleReportRowDto
                {
                    UserId = g.Key.AssignedUserId,
                    UserName = g.Key.AssignedUserName,
                    ScheduleCount = g.Count(),
                    RegularHours = Round(regularHours),
                    OvertimeHours = Round(overtimeHours),
                    TotalHours = Round(regularHours + overtimeHours),
                    RegularPay = Round(regularPay),
                    OvertimePay = Round(overtimePay),
                    TotalPay = Round(regularPay + overtimePay),
                };
            })
            .OrderBy(r => r.UserName)
            .ToList();

        return new ScheduleReportDto
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Rows = grouped,
            TotalRegularHours = Round(grouped.Sum(r => r.RegularHours)),
            TotalOvertimeHours = Round(grouped.Sum(r => r.OvertimeHours)),
            TotalHours = Round(grouped.Sum(r => r.TotalHours)),
            TotalPay = Round(grouped.Sum(r => r.TotalPay)),
        };
    }

    // ---------------------------------------------------------- Live location

    public async Task RecordLocationAsync(int scheduleId, RecordLocationRequestDto request, int currentUserId, int roleId, int agencyId, CancellationToken ct = default)
    {
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        if (schedule.AssignedUserId != currentUserId)
            throw AppException.Forbidden("Only the assigned user can share location for this schedule.");

        // Location is only shared while actually clocked in on the task.
        var open = await _schedules.GetOpenTimeEntryAsync(scheduleId, currentUserId, ct);
        if (open is null)
            throw AppException.BadRequest("You must be clocked in to share your location.");

        var pingId = await _locations.CreateAsync(new LocationPing
        {
            ScheduleId = scheduleId,
            UserId = currentUserId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
        }, ct);

        var live = new LiveLocationDto
        {
            ScheduleId = scheduleId,
            Title = schedule.Title,
            UserId = currentUserId,
            UserName = schedule.AssignedUserName ?? string.Empty,
            CustomerName = schedule.CustomerName,
            Location = schedule.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
            RecordedUtc = DateTime.UtcNow,
        };

        try
        {
            await _broadcaster.BroadcastLiveLocationAsync(agencyId, live, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stored location ping {PingId} but failed to broadcast it.", pingId);
        }
    }

    public async Task<IReadOnlyList<LocationPingDto>> GetTrailAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");

        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var pings = await _locations.GetTrailAsync(scheduleId, ct);
        return pings.Select(p => new LocationPingDto
        {
            PingId = p.PingId,
            ScheduleId = p.ScheduleId,
            UserId = p.UserId,
            UserName = p.UserName ?? string.Empty,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            AccuracyMeters = p.AccuracyMeters,
            RecordedUtc = p.RecordedUtc,
        }).ToList();
    }

    public async Task<IReadOnlyList<LiveLocationDto>> GetLiveLocationsAsync(int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        if (level == None)
            throw AppException.Forbidden("You do not have access to live locations.");

        int? scope = level == Self ? currentUserId : null;
        var rows = await _locations.GetLiveLatestAsync(scope, ct);
        return rows.Select(r => new LiveLocationDto
        {
            ScheduleId = r.ScheduleId,
            Title = r.Title,
            UserId = r.UserId,
            UserName = r.UserName,
            CustomerName = r.CustomerName,
            Location = r.Location,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            AccuracyMeters = r.AccuracyMeters,
            RecordedUtc = r.RecordedUtc,
        }).ToList();
    }

    // ------------------------------------------------------------------ Emails

    private async Task SendCreationEmailsAsync(Schedule schedule, UserDto assignee, bool notifyAdmin, bool notifyManager, CancellationToken ct)
    {
        // The assigned user is always notified.
        try
        {
            await _email.SendScheduleAssignedAsync(assignee.Email, assignee.FullName, schedule.Title,
                schedule.Location, schedule.StartUtc, schedule.EndUtc, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send schedule-assigned email to {Email}.", assignee.Email);
        }

        // Also text them if they have a phone on file and SMS is configured.
        if (!string.IsNullOrWhiteSpace(assignee.Phone))
        {
            try
            {
                var when = schedule.StartUtc.ToString("MMM d, h:mm tt");
                await _sms.SendAsync(assignee.Phone!,
                    $"Hi {assignee.FullName}, you've been assigned \"{schedule.Title}\" starting {when}. — WorkProvider360", ct);
            }
            catch (Exception ex)
            {
                // SMS is best-effort — never block schedule creation on it.
                _logger.LogWarning(ex, "Failed to send schedule-assigned SMS to {Phone}.", assignee.Phone);
            }
        }

        if (!notifyAdmin && !notifyManager) return;

        try
        {
            var users = await _users.GetAllAsync(ct);
            var recipients = users.Where(u => u.IsActive && (
                    (notifyAdmin && u.RoleName == RoleConstants.Admin) ||
                    (notifyManager && u.RoleName == RoleConstants.Manager)))
                .Select(u => u.Email)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var to in recipients)
            {
                try
                {
                    await _email.SendScheduleNotificationAsync(to, schedule.Title,
                        schedule.AssignedUserName ?? assignee.FullName, schedule.StartUtc, schedule.EndUtc, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send schedule notification to {Email}.", to);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin/manager recipients for schedule notification.");
        }
    }

    private async Task SendInjuryEmailsAsync(Schedule schedule, string reporterName, string message, CancellationToken ct)
    {
        try
        {
            var users = await _users.GetAllAsync(ct);
            var recipients = users.Where(u => u.IsActive &&
                    (u.RoleName == RoleConstants.SuperAdmin || u.RoleName == RoleConstants.Admin || u.RoleName == RoleConstants.Manager))
                .Select(u => u.Email)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var to in recipients)
            {
                try
                {
                    await _email.SendScheduleInjuryReportAsync(to, schedule.Title, reporterName, message, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send injury report email to {Email}.", to);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load recipients for injury report on schedule {Id}.", schedule.ScheduleId);
        }
    }

    // ------------------------------------------------------- Permission helpers

    /// <summary>When the tenant has auto-clock on, lazily resolve missed, ended shifts.</summary>
    private async Task MaybeAutoClockAsync(SchedulingSettings? settings, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (settings?.AutoClockEnabled == true)
            await _schedules.ApplyAutoClockAsync(now, ct);

        // Client-visit auto clock-in/out phases are driven by ClientSettings.
        var cs = await _clientSettings.GetAsync(ct);
        if (cs is not null && (cs.AutoClockInEnabled || cs.AutoClockOutEnabled))
            await _schedules.ApplyAutoClockPhasesAsync(now, cs.AutoClockInEnabled, cs.AutoClockOutEnabled, ct);
    }

    /// <summary>Role rank: SuperAdmin=1 (highest) … User=4. Unknown roles rank lowest.</summary>
    private static int RankOfRoleId(int roleId) => roleId switch
    {
        RoleConstants.SuperAdminId => 1,
        RoleConstants.AdminId => 2,
        RoleConstants.ManagerId => 3,
        RoleConstants.UserId => 4,
        RoleConstants.ClientId => 5,
        _ => int.MaxValue,
    };

    private static int RankOfRoleName(string? roleName) => roleName switch
    {
        RoleConstants.SuperAdmin => 1,
        RoleConstants.Admin => 2,
        RoleConstants.Manager => 3,
        RoleConstants.User => 4,
        RoleConstants.Client => 5,
        _ => int.MaxValue,
    };

    /// <summary>You may only schedule someone who ranks strictly below you.</summary>
    private static void EnsureCanScheduleFor(int roleId, UserDto assignee)
    {
        if (RankOfRoleName(assignee.RoleName) <= RankOfRoleId(roleId))
            throw AppException.Forbidden("You cannot schedule someone at or above your role.");
    }

    private static string LevelForRole(int roleId, SchedulingSettings? settings)
    {
        if (roleId == RoleConstants.SuperAdminId) return Write;
        if (roleId == RoleConstants.AdminId) return CanonLevel(settings?.AdminAccess ?? Write);
        if (roleId == RoleConstants.ManagerId) return CanonLevel(settings?.ManagerAccess ?? Read);
        if (roleId == RoleConstants.UserId) return Self;
        return None;
    }

    private static bool CanManage(int roleId, SchedulingSettings? settings)
        => LevelForRole(roleId, settings) == Write;

    private static void EnsureCanManage(int roleId, SchedulingSettings? settings)
    {
        if (!CanManage(roleId, settings))
            throw AppException.Forbidden("You do not have permission to manage schedules.");
    }

    /// <summary>View a specific schedule: managers/readers see all; a User only their own.</summary>
    private static void EnsureCanViewSchedule(int roleId, SchedulingSettings? settings, Schedule schedule, int currentUserId)
    {
        var level = LevelForRole(roleId, settings);
        if (level is Read or Write) return;
        if (level == Self && schedule.AssignedUserId == currentUserId) return;
        throw AppException.Forbidden("You do not have access to this schedule.");
    }

    private async Task<Schedule> RequireAssignedScheduleAsync(int scheduleId, int currentUserId, CancellationToken ct)
    {
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");
        if (schedule.AssignedUserId != currentUserId)
            throw AppException.Forbidden("Only the assigned user can track time on this schedule.");
        return schedule;
    }

    private static SchedulingAccessDto BuildAccess(int roleId, string level) => new()
    {
        RoleName = roleId switch
        {
            RoleConstants.SuperAdminId => RoleConstants.SuperAdmin,
            RoleConstants.AdminId => RoleConstants.Admin,
            RoleConstants.ManagerId => RoleConstants.Manager,
            RoleConstants.UserId => RoleConstants.User,
            _ => string.Empty,
        },
        AccessLevel = level,
        IsSuperAdmin = roleId == RoleConstants.SuperAdminId,
        CanViewAll = level is Read or Write,
        CanManage = level == Write,
        // Admin outranks Manager and can set the Manager's access; SuperAdmin sets both.
        CanManageAccess = roleId is RoleConstants.SuperAdminId or RoleConstants.AdminId,
        IsSelfScoped = level == Self,
    };

    private static string CanonLevel(string? value)
    {
        if (string.Equals(value, Write, StringComparison.OrdinalIgnoreCase)) return Write;
        if (string.Equals(value, Read, StringComparison.OrdinalIgnoreCase)) return Read;
        return None;
    }

    // ------------------------------------------------------------- Map / helpers

    private static void ValidateWindow(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
            throw AppException.BadRequest("The end time must be after the start time.");
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static ScheduleDto MapSchedule(Schedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        Title = s.Title,
        CustomerName = s.CustomerName,
        Location = s.Location,
        ClientId = s.ClientId,
        ClientName = s.ClientName,
        ServiceTypeId = s.ServiceTypeId,
        ServiceTypeName = s.ServiceTypeName,
        AssignedUserId = s.AssignedUserId,
        AssignedUserName = s.AssignedUserName ?? string.Empty,
        StartUtc = s.StartUtc,
        EndUtc = s.EndUtc,
        PayRatePerHour = s.PayRatePerHour,
        OvertimeMultiplier = s.OvertimeMultiplier,
        Status = s.Status,
        RejectionReason = s.RejectionReason,
        ColorTag = s.ColorTag,
        CreatedByUserId = s.CreatedByUserId,
        CreatedOn = s.CreatedOn,
        UpdatedOn = s.UpdatedOn,
    };

    private static ScheduleNoteDto MapNote(ScheduleNote n) => new()
    {
        NoteId = n.NoteId,
        ScheduleId = n.ScheduleId,
        AuthorUserId = n.AuthorUserId,
        AuthorName = n.AuthorName ?? string.Empty,
        NoteType = n.NoteType,
        Message = n.Message,
        CreatedOn = n.CreatedOn,
    };

    private static TimeEntryDto MapTime(TimeEntry t) => new()
    {
        TimeEntryId = t.TimeEntryId,
        ScheduleId = t.ScheduleId,
        UserId = t.UserId,
        UserName = t.UserName ?? string.Empty,
        ClockInUtc = t.ClockInUtc,
        ClockOutUtc = t.ClockOutUtc,
        Source = t.Source,
        Note = t.Note,
        Hours = t.ClockOutUtc is null
            ? 0
            : Round((decimal)(t.ClockOutUtc.Value - t.ClockInUtc).TotalHours),
    };

    private static SchedulingSettingsDto MapSettings(SchedulingSettings? s) => new()
    {
        AdminAccess = CanonLevel(s?.AdminAccess ?? Write),
        ManagerAccess = CanonLevel(s?.ManagerAccess ?? Read),
        DefaultPayRatePerHour = s?.DefaultPayRatePerHour ?? 0,
        DefaultOvertimeMultiplier = s?.DefaultOvertimeMultiplier ?? 1.5m,
        NotifyAdminOnCreate = s?.NotifyAdminOnCreate ?? false,
        NotifyManagerOnCreate = s?.NotifyManagerOnCreate ?? false,
        AutoClockEnabled = s?.AutoClockEnabled ?? false,
        UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
    };

    // ------------------------------------------------------ Client / signatures

    public async Task<IReadOnlyList<TimeEntrySignatureDto>> GetSignaturesAsync(int scheduleId, int timeEntryId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");
        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var sigs = await _schedules.GetSignaturesAsync(timeEntryId, ct);
        return sigs.Select(s => new TimeEntrySignatureDto
        {
            SignatureId = s.SignatureId,
            TimeEntryId = s.TimeEntryId,
            Phase = s.Phase,
            SignatureBase64 = s.SignatureBase64,
            SignedByName = s.SignedByName,
            SignedOnUtc = s.SignedOnUtc,
        }).ToList();
    }

    /// <summary>
    /// Aggregated, chronological care log for a shift: clock-in/out events (with
    /// GPS), notes and injury reports, and captured client signatures with their
    /// images. Visible to staff who can view the schedule.
    /// </summary>
    public async Task<IReadOnlyList<CareLogEntryDto>> GetCareLogAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var schedule = await _schedules.GetByIdAsync(scheduleId, ct)
            ?? throw AppException.NotFound("Schedule not found.");
        EnsureCanViewSchedule(roleId, settings, schedule, currentUserId);

        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        var notes = await _schedules.GetNotesAsync(scheduleId, ct);

        var log = new List<CareLogEntryDto>();

        foreach (var e in entries)
        {
            log.Add(new CareLogEntryDto
            {
                Type = "ClockIn",
                TimestampUtc = e.ClockInUtc,
                ActorName = e.UserName ?? string.Empty,
                Source = e.Source,
                Latitude = e.ClockInLatitude,
                Longitude = e.ClockInLongitude,
            });

            if (e.ClockOutUtc is not null)
            {
                log.Add(new CareLogEntryDto
                {
                    Type = "ClockOut",
                    TimestampUtc = e.ClockOutUtc.Value,
                    ActorName = e.UserName ?? string.Empty,
                    Source = e.Source,
                    Latitude = e.ClockOutLatitude,
                    Longitude = e.ClockOutLongitude,
                });
            }

            // Signatures captured against this entry (image included).
            var sigs = await _schedules.GetSignaturesAsync(e.TimeEntryId, ct);
            foreach (var s in sigs)
            {
                log.Add(new CareLogEntryDto
                {
                    Type = "Signature",
                    TimestampUtc = s.SignedOnUtc,
                    ActorName = s.SignedByName ?? (e.UserName ?? string.Empty),
                    Phase = s.Phase,
                    SignedByName = s.SignedByName,
                    SignatureBase64 = s.SignatureBase64,
                });
            }
        }

        foreach (var n in notes)
        {
            log.Add(new CareLogEntryDto
            {
                Type = string.Equals(n.NoteType, "Injury", StringComparison.OrdinalIgnoreCase) ? "Injury" : "Note",
                TimestampUtc = n.CreatedOn,
                ActorName = n.AuthorName ?? string.Empty,
                Message = n.Message,
            });
        }

        return log.OrderBy(x => x.TimestampUtc).ToList();
    }


    /// <summary>
    /// Resolves the client for a schedule and the effective CustomerName/Location.
    /// When a client is chosen these are taken from the client record; the same-office
    /// and matching-skill rules from ClientSettings are enforced against the assignee.
    /// </summary>
    private async Task<(Client? client, string? customerName, string? location)> ResolveClientForScheduleAsync(
        int? clientId, int? serviceTypeId, string? customerName, string? location, UserDto assignee, CancellationToken ct)
    {
        if (clientId is null)
            return (null, Clean(customerName), Clean(location));

        var client = await _clients.GetByIdAsync(clientId.Value, ct)
            ?? throw AppException.BadRequest("The selected client does not exist.");

        var cs = await _clientSettings.GetAsync(ct);
        if (cs is not null && (cs.RequireSameOffice || cs.RequireMatchingSkill))
        {
            var eligible = await _clients.GetEligibleCaregiversAsync(clientId.Value, serviceTypeId, ct);
            var match = eligible.FirstOrDefault(e => e.UserId == assignee.UserId);
            if (cs.RequireSameOffice && (match is null || !match.IsSameOffice))
                throw AppException.BadRequest("The assigned team member must belong to the same office as the client.");
            if (cs.RequireMatchingSkill && serviceTypeId is not null && (match is null || !match.HasSkill))
                throw AppException.BadRequest("The assigned team member does not have the required skill for this service.");
        }

        var name = $"{client.FirstName} {client.LastName}".Trim();
        var addr = string.Join(", ", new[] { client.AddressLine1, client.AddressLine2, client.City, client.State, client.PostalCode }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        return (client, name, string.IsNullOrWhiteSpace(addr) ? Clean(location) : addr);
    }

    private async Task MaybeNotifyClientAsync(Schedule schedule, Client? client, CancellationToken ct)
    {
        if (client is null || string.IsNullOrWhiteSpace(client.Email)) return;
        var cs = await _clientSettings.GetAsync(ct);
        if (cs?.NotifyClientOnSchedule != true) return;
        try
        {
            await _email.SendClientVisitScheduledAsync(client.Email!, $"{client.FirstName} {client.LastName}".Trim(),
                schedule.Title, schedule.ServiceTypeName, schedule.AssignedUserName ?? string.Empty,
                schedule.StartUtc, schedule.EndUtc, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled visit {Id} but failed to notify the client.", schedule.ScheduleId);
        }
    }

    /// <summary>Validates and returns the signature to persist, enforcing required-signature flags for client visits.</summary>
    private async Task<(string Base64, string? Name)?> ResolveSignatureAsync(Schedule schedule, ClockRequestDto? request, string phase, CancellationToken ct)
    {
        var sig = Clean(request?.SignatureBase64);

        if (schedule.ClientId is not null)
        {
            var cs = await _clientSettings.GetAsync(ct);
            var required = phase == "ClockIn"
                ? cs?.RequireClientSignatureOnClockIn ?? false
                : cs?.RequireClientSignatureOnClockOut ?? false;
            if (required && sig is null)
                throw AppException.BadRequest($"A client signature is required to clock {(phase == "ClockIn" ? "in" : "out")}.");
        }

        return sig is null ? null : (sig, Clean(request?.SignedByName));
    }

    private async Task SaveSignatureAsync(int timeEntryId, string phase, (string Base64, string? Name)? signature, CancellationToken ct)
    {
        if (signature is null || timeEntryId == 0) return;
        await _schedules.CreateSignatureAsync(new TimeEntrySignature
        {
            TimeEntryId = timeEntryId,
            Phase = phase,
            SignatureBase64 = signature.Value.Base64,
            SignedByName = signature.Value.Name,
        }, ct);
    }

}
