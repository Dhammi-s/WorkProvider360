using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>
/// Scheduling data access against the current TENANT database using stored procedures.
/// </summary>
public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ScheduleRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    // -------------------------------------------------------------- Schedules

    public async Task<int> CreateAsync(Schedule schedule, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Schedule_Create",
                new
                {
                    schedule.Title,
                    schedule.CustomerName,
                    schedule.Location,
                    schedule.AssignedUserId,
                    schedule.StartUtc,
                    schedule.EndUtc,
                    schedule.PayRatePerHour,
                    schedule.OvertimeMultiplier,
                    schedule.ColorTag,
                    schedule.CreatedByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Schedule>> GetAllAsync(DateTime? fromUtc, DateTime? toUtc, int? assignedUserId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Schedule>(
            new CommandDefinition("usp_Schedule_GetAll",
                new { FromUtc = fromUtc, ToUtc = toUtc, AssignedUserId = assignedUserId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<Schedule?> GetByIdAsync(int scheduleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Schedule>(
            new CommandDefinition("usp_Schedule_GetById", new { ScheduleId = scheduleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(Schedule schedule, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Schedule_Update",
                new
                {
                    schedule.ScheduleId,
                    schedule.Title,
                    schedule.CustomerName,
                    schedule.Location,
                    schedule.AssignedUserId,
                    schedule.StartUtc,
                    schedule.EndUtc,
                    schedule.PayRatePerHour,
                    schedule.OvertimeMultiplier,
                    schedule.ColorTag
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateStatusAsync(int scheduleId, string status, string? rejectionReason, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Schedule_UpdateStatus",
                new { ScheduleId = scheduleId, Status = status, RejectionReason = rejectionReason },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task DeleteAsync(int scheduleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Schedule_Delete", new { ScheduleId = scheduleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    // ------------------------------------------------------------------ Notes

    public async Task<int> CreateNoteAsync(ScheduleNote note, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_ScheduleNote_Create",
                new { note.ScheduleId, note.AuthorUserId, note.NoteType, note.Message },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ScheduleNote>> GetNotesAsync(int scheduleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ScheduleNote>(
            new CommandDefinition("usp_ScheduleNote_GetBySchedule", new { ScheduleId = scheduleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    // ------------------------------------------------------------ Time entries

    public async Task<int> ClockInAsync(int scheduleId, int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_TimeEntry_ClockIn",
                new { ScheduleId = scheduleId, UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> ClockOutAsync(int scheduleId, int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_TimeEntry_ClockOut",
                new { ScheduleId = scheduleId, UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<TimeEntry?> GetOpenTimeEntryAsync(int scheduleId, int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<TimeEntry>(
            new CommandDefinition("usp_TimeEntry_GetOpen",
                new { ScheduleId = scheduleId, UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> CreateTimeEntryAsync(TimeEntry entry, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_TimeEntry_Create",
                new { entry.ScheduleId, entry.UserId, entry.ClockInUtc, entry.ClockOutUtc, entry.Note },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateTimeEntryAsync(TimeEntry entry, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_TimeEntry_Update",
                new { entry.TimeEntryId, entry.ClockInUtc, entry.ClockOutUtc, entry.Note },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<TimeEntry>> GetTimeEntriesAsync(int scheduleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<TimeEntry>(
            new CommandDefinition("usp_TimeEntry_GetBySchedule", new { ScheduleId = scheduleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    // -------------------------------------------------------------- Reporting

    public async Task<IReadOnlyList<ScheduleReportRow>> GetReportAsync(DateTime fromUtc, DateTime toUtc, int? assignedUserId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ScheduleReportRow>(
            new CommandDefinition("usp_Schedule_GetReport",
                new { FromUtc = fromUtc, ToUtc = toUtc, AssignedUserId = assignedUserId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
