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
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(
        IScheduleRepository schedules,
        ISchedulingSettingsRepository settings,
        ILocationRepository locations,
        ILocationBroadcaster broadcaster,
        IUserService users,
        IEmailService email,
        ILogger<SchedulingService> logger)
    {
        _schedules = schedules;
        _settings = settings;
        _locations = locations;
        _broadcaster = broadcaster;
        _users = users;
        _email = email;
        _logger = logger;
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
        if (roleId != RoleConstants.SuperAdminId)
            throw AppException.Forbidden("Only a Super Admin can change scheduling access.");

        var saved = await _settings.UpdateAccessAsync(
            CanonLevel(request.AdminAccess), CanonLevel(request.ManagerAccess), ct);
        return MapSettings(saved);
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
            ct);
        return MapSettings(saved);
    }

    public async Task<IReadOnlyList<UserDto>> GetAssignableUsersAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);

        var users = await _users.GetAllAsync(ct);
        return users.Where(u => u.IsActive).ToList();
    }

    // --------------------------------------------------------------- Schedules

    public async Task<IReadOnlyList<ScheduleDto>> GetSchedulesAsync(
        DateTime? fromUtc, DateTime? toUtc, int? assignedUserId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        if (level == None)
            throw AppException.Forbidden("You do not have access to the scheduler.");

        // A regular User only ever sees their own schedules.
        var effectiveUserId = level == Self ? currentUserId : assignedUserId;

        var rows = await _schedules.GetAllAsync(fromUtc, toUtc, effectiveUserId, ct);
        return rows.Select(MapSchedule).ToList();
    }

    public async Task<ScheduleDetailDto> GetScheduleAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
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

        var id = await _schedules.CreateAsync(new Schedule
        {
            Title = request.Title.Trim(),
            CustomerName = Clean(request.CustomerName),
            Location = Clean(request.Location),
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

        await _schedules.UpdateAsync(new Schedule
        {
            ScheduleId = scheduleId,
            Title = request.Title.Trim(),
            CustomerName = Clean(request.CustomerName),
            Location = Clean(request.Location),
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

    public async Task ClockInAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var schedule = await RequireAssignedScheduleAsync(scheduleId, currentUserId, ct);

        var open = await _schedules.GetOpenTimeEntryAsync(scheduleId, currentUserId, ct);
        if (open is not null)
            throw AppException.BadRequest("You are already clocked in for this schedule.");

        await _schedules.ClockInAsync(scheduleId, currentUserId, ct);

        // Reflect that work has started (unless already completed/cancelled).
        if (schedule.Status is "Scheduled" or "Accepted")
            await _schedules.UpdateStatusAsync(scheduleId, "InProgress", schedule.RejectionReason, ct);
    }

    public async Task ClockOutAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await RequireAssignedScheduleAsync(scheduleId, currentUserId, ct);

        var affected = await _schedules.ClockOutAsync(scheduleId, currentUserId, ct);
        if (affected == 0)
            throw AppException.BadRequest("You are not currently clocked in for this schedule.");

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
        CanManageAccess = roleId == RoleConstants.SuperAdminId,
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
        UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
    };
}
