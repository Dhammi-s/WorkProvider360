/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Office meeting data access against the current TENANT database.</summary>
public interface IMeetingRepository
{
    // Settings
    Task<MeetingSettings> GetSettingsAsync(CancellationToken ct = default);
    Task<MeetingSettings> UpdateSettingsAsync(MeetingSettings settings, CancellationToken ct = default);

    // Meetings
    Task<int> CreateAsync(Meeting meeting, CancellationToken ct = default);
    Task<IReadOnlyList<Meeting>> GetAllAsync(
        DateTime? fromUtc, DateTime? toUtc, string? status,
        int? createdByUserId, int? participantUserId, int? participantClientId,
        CancellationToken ct = default);
    Task<Meeting?> GetByIdAsync(int meetingId, CancellationToken ct = default);
    Task UpdateAsync(Meeting meeting, CancellationToken ct = default);
    Task UpdateStatusAsync(int meetingId, string status, CancellationToken ct = default);
    Task DeleteAsync(int meetingId, CancellationToken ct = default);

    // Participants
    Task<int> AddParticipantAsync(int meetingId, int? userId, int? clientId, string participantRole, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingParticipant>> GetParticipantsAsync(int meetingId, CancellationToken ct = default);
    Task RemoveParticipantAsync(int participantId, CancellationToken ct = default);
    Task<int> RespondParticipantAsync(int meetingId, int userId, string status, CancellationToken ct = default);

    // Payments
    Task<int> RecordPaymentAsync(MeetingPayment payment, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingPayment>> GetPaymentsAsync(int meetingId, CancellationToken ct = default);
}
