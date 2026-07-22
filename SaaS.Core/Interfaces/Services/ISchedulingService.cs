using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Scheduling workflow: schedules, the assigned user's responses (accept/reject/
/// injury/notes), time tracking, reporting, and role-based access settings.
/// Every method that touches data enforces the caller's effective permission.
/// </summary>
public interface ISchedulingService
{
    // Access / settings
    Task<SchedulingAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default);
    Task<SchedulingSettingsDto> GetSettingsAsync(int roleId, CancellationToken ct = default);
    Task<SchedulingSettingsDto> UpdateAccessAsync(UpdateSchedulingAccessDto request, int roleId, CancellationToken ct = default);
    Task<SchedulingSettingsDto> UpdateDefaultsAsync(UpdateSchedulingDefaultsDto request, int roleId, CancellationToken ct = default);

    /// <summary>Active users that a manager can assign schedules to. Requires manage permission.</summary>
    Task<IReadOnlyList<UserDto>> GetAssignableUsersAsync(int roleId, CancellationToken ct = default);

    // Schedules
    Task<IReadOnlyList<ScheduleDto>> GetSchedulesAsync(DateTime? fromUtc, DateTime? toUtc, int? assignedUserId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ScheduleDetailDto> GetScheduleAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ScheduleDto> CreateAsync(CreateScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ScheduleDto> UpdateAsync(int scheduleId, UpdateScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task DeleteAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);

    // Assigned-user actions
    Task<ScheduleDto> RespondAsync(int scheduleId, RespondScheduleRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ScheduleNoteDto> AddNoteAsync(int scheduleId, CreateScheduleNoteRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduleNoteDto>> GetNotesAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);

    // Time tracking
    Task ClockInAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);
    Task ClockOutAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<TimeEntryDto> AddManualTimeAsync(int scheduleId, ManualTimeEntryRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<TimeEntryDto> UpdateTimeAsync(int scheduleId, int timeEntryId, ManualTimeEntryRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<TimeEntryDto>> GetTimeEntriesAsync(int scheduleId, int currentUserId, int roleId, CancellationToken ct = default);

    // Reporting
    Task<ScheduleReportDto> GetReportAsync(DateTime fromUtc, DateTime toUtc, int? assignedUserId, int currentUserId, int roleId, CancellationToken ct = default);
}
