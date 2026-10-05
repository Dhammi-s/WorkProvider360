/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>Business logic for the office meeting scheduling feature.</summary>
public interface IMeetingService
{
    // Settings
    Task<MeetingSettingsDto> GetSettingsAsync(int callerRoleId, CancellationToken ct = default);
    Task<MeetingSettingsDto> UpdateSettingsAsync(UpdateMeetingSettingsRequestDto request, int callerRoleId, CancellationToken ct = default);
    Task<MeetingAccessDto> GetAccessAsync(int callerRoleId, CancellationToken ct = default);

    // Meetings
    Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequestDto request, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingDto>> GetMeetingsAsync(DateTime? from, DateTime? to, string? status, int? userId, int? clientId, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<MeetingDetailDto> GetMeetingByIdAsync(int meetingId, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<MeetingDto> UpdateMeetingAsync(int meetingId, UpdateMeetingRequestDto request, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<MeetingDto> UpdateStatusAsync(int meetingId, string status, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task DeleteMeetingAsync(int meetingId, int callerUserId, int callerRoleId, CancellationToken ct = default);

    // Participants
    Task<MeetingParticipantDto> AddParticipantAsync(int meetingId, AddMeetingParticipantRequestDto request, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingParticipantDto>> GetParticipantsAsync(int meetingId, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task RemoveParticipantAsync(int meetingId, int participantId, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<bool> RespondAsync(int meetingId, RespondMeetingRequestDto request, int callerUserId, CancellationToken ct = default);

    // Payments
    Task<MeetingPaymentDto> RecordPaymentAsync(int meetingId, RecordMeetingPaymentRequestDto request, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingPaymentDto>> GetPaymentsAsync(int meetingId, int callerUserId, int callerRoleId, CancellationToken ct = default);
    Task<MeetingPaymentSummaryDto> GetPaymentSummaryAsync(int meetingId, int callerUserId, int callerRoleId, CancellationToken ct = default);
}
