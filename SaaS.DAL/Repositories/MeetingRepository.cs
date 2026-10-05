/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>
/// Office meeting data access against the current TENANT database using stored procedures.
/// </summary>
public sealed class MeetingRepository : IMeetingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MeetingRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    // ------------------------------------------------------ Settings

    public async Task<MeetingSettings> GetSettingsAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<MeetingSettings>(
            new CommandDefinition("usp_MeetingSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<MeetingSettings> UpdateSettingsAsync(MeetingSettings s, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<MeetingSettings>(
            new CommandDefinition("usp_MeetingSettings_Update",
                new
                {
                    s.AdminAccess,
                    s.ManagerAccess,
                    s.UserCanCreate,
                    s.AllowClientParticipants,
                    s.AllowPaidMeetings,
                    s.DefaultFeePerParticipant,
                    s.RequireApproval,
                    s.NotifyOnCreate,
                    s.NotifyOnUpdate,
                    s.NotifyOnCancel,
                    s.MaxParticipantsDefault
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    // ------------------------------------------------------ Meetings

    public async Task<int> CreateAsync(Meeting meeting, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Meeting_Create",
                new
                {
                    meeting.Title,
                    meeting.Description,
                    meeting.StartUtc,
                    meeting.EndUtc,
                    meeting.Location,
                    meeting.MeetingType,
                    meeting.IsPaid,
                    meeting.FeePerParticipant,
                    meeting.CreatedByUserId,
                    meeting.MaxParticipants,
                    meeting.Notes,
                    meeting.ColorTag
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Meeting>> GetAllAsync(
        DateTime? fromUtc, DateTime? toUtc, string? status,
        int? createdByUserId, int? participantUserId, int? participantClientId,
        CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Meeting>(
            new CommandDefinition("usp_Meeting_GetAll",
                new
                {
                    FromUtc              = fromUtc,
                    ToUtc                = toUtc,
                    Status               = status,
                    CreatedByUserId      = createdByUserId,
                    ParticipantUserId    = participantUserId,
                    ParticipantClientId  = participantClientId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<Meeting?> GetByIdAsync(int meetingId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Meeting>(
            new CommandDefinition("usp_Meeting_GetById",
                new { MeetingId = meetingId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(Meeting meeting, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Meeting_Update",
                new
                {
                    meeting.MeetingId,
                    meeting.Title,
                    meeting.Description,
                    meeting.StartUtc,
                    meeting.EndUtc,
                    meeting.Location,
                    meeting.MeetingType,
                    meeting.IsPaid,
                    meeting.FeePerParticipant,
                    meeting.MaxParticipants,
                    meeting.Notes,
                    meeting.ColorTag
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateStatusAsync(int meetingId, string status, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Meeting_UpdateStatus",
                new { MeetingId = meetingId, Status = status },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task DeleteAsync(int meetingId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Meeting_Delete",
                new { MeetingId = meetingId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    // ------------------------------------------------------ Participants

    public async Task<int> AddParticipantAsync(int meetingId, int? userId, int? clientId, string participantRole, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Meeting_AddParticipant",
                new { MeetingId = meetingId, UserId = userId, ClientId = clientId, ParticipantRole = participantRole },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<MeetingParticipant>> GetParticipantsAsync(int meetingId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<MeetingParticipant>(
            new CommandDefinition("usp_Meeting_GetParticipants",
                new { MeetingId = meetingId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task RemoveParticipantAsync(int participantId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Meeting_RemoveParticipant",
                new { ParticipantId = participantId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> RespondParticipantAsync(int meetingId, int userId, string status, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Meeting_RespondParticipant",
                new { MeetingId = meetingId, UserId = userId, Status = status },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    // ------------------------------------------------------ Payments

    public async Task<int> RecordPaymentAsync(MeetingPayment payment, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Meeting_RecordPayment",
                new
                {
                    payment.MeetingId,
                    payment.ParticipantId,
                    payment.Amount,
                    payment.Method,
                    payment.Status,
                    payment.TransactionId,
                    payment.Notes,
                    payment.RecordedByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<MeetingPayment>> GetPaymentsAsync(int meetingId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<MeetingPayment>(
            new CommandDefinition("usp_Meeting_GetPayments",
                new { MeetingId = meetingId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
