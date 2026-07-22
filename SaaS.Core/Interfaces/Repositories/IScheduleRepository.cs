using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Scheduling data access against the current TENANT database.</summary>
public interface IScheduleRepository
{
    // Schedules
    Task<int> CreateAsync(Schedule schedule, CancellationToken ct = default);
    Task<IReadOnlyList<Schedule>> GetAllAsync(DateTime? fromUtc, DateTime? toUtc, int? assignedUserId, CancellationToken ct = default);
    Task<Schedule?> GetByIdAsync(int scheduleId, CancellationToken ct = default);
    Task UpdateAsync(Schedule schedule, CancellationToken ct = default);
    Task UpdateStatusAsync(int scheduleId, string status, string? rejectionReason, CancellationToken ct = default);
    Task DeleteAsync(int scheduleId, CancellationToken ct = default);

    // Notes
    Task<int> CreateNoteAsync(ScheduleNote note, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduleNote>> GetNotesAsync(int scheduleId, CancellationToken ct = default);

    // Time entries
    Task<int> ClockInAsync(int scheduleId, int userId, CancellationToken ct = default);
    Task<int> ClockOutAsync(int scheduleId, int userId, CancellationToken ct = default);
    Task<TimeEntry?> GetOpenTimeEntryAsync(int scheduleId, int userId, CancellationToken ct = default);
    Task<int> CreateTimeEntryAsync(TimeEntry entry, CancellationToken ct = default);
    Task UpdateTimeEntryAsync(TimeEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<TimeEntry>> GetTimeEntriesAsync(int scheduleId, CancellationToken ct = default);

    // Reporting
    Task<IReadOnlyList<ScheduleReportRow>> GetReportAsync(DateTime fromUtc, DateTime toUtc, int? assignedUserId, CancellationToken ct = default);
}
