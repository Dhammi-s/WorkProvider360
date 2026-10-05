/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Office meeting scheduling: create meetings between users, clients, admins,
/// managers and super-admin; RSVP; paid meeting payment tracking; settings.
///
/// Access matrix (all role-based, further governed by MeetingSettings):
///   SuperAdmin  — full access: create / edit / delete / manage payments / settings
///   Admin       — governed by MeetingSettings.AdminAccess
///   Manager     — governed by MeetingSettings.ManagerAccess
///   User        — view own meetings + RSVP; create only when UserCanCreate = true
///   Client      — view own invitations + RSVP only
/// </summary>
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.User},{RoleConstants.Client}")]
public sealed class MeetingsController : BaseApiController
{
    private readonly IMeetingService _meetings;

    public MeetingsController(IMeetingService meetings) => _meetings = meetings;

    // ============================================================ Settings

    /// <summary>Current user's effective meeting access level.</summary>
    [HttpGet("access")]
    public async Task<ActionResult<ApiResponse<MeetingAccessDto>>> GetAccess(CancellationToken ct)
    {
        var access = await _meetings.GetAccessAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingAccessDto>.Ok(access));
    }

    /// <summary>Retrieve current tenant meeting settings. All authenticated roles can view.</summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<MeetingSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _meetings.GetSettingsAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingSettingsDto>.Ok(settings));
    }

    /// <summary>Update meeting settings. SuperAdmin only.</summary>
    [HttpPut("settings")]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<MeetingSettingsDto>>> UpdateSettings(
        [FromBody] UpdateMeetingSettingsRequestDto request, CancellationToken ct)
    {
        var settings = await _meetings.UpdateSettingsAsync(request, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingSettingsDto>.Ok(settings, "Meeting settings updated."));
    }

    // ============================================================ Meetings

    /// <summary>
    /// List meetings. SuperAdmin/Admin/Manager see all (filterable).
    /// Users see only meetings they are participating in.
    /// Clients see only their own invitations.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MeetingDto>>>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string?   status,
        [FromQuery] int?      userId,
        [FromQuery] int?      clientId,
        CancellationToken ct)
    {
        var items = await _meetings.GetMeetingsAsync(
            from, to, status, userId, clientId, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<MeetingDto>>.Ok(items));
    }

    /// <summary>Get a meeting's full detail including participants.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MeetingDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var detail = await _meetings.GetMeetingByIdAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingDetailDto>.Ok(detail));
    }

    /// <summary>Create a new meeting. Role + settings determine if allowed.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MeetingDto>>> Create(
        [FromBody] CreateMeetingRequestDto request, CancellationToken ct)
    {
        var meeting = await _meetings.CreateMeetingAsync(request, CurrentUserId, CurrentRoleId, ct);
        return CreatedAtAction(nameof(GetById), new { id = meeting.MeetingId },
            ApiResponse<MeetingDto>.Ok(meeting, "Meeting created."));
    }

    /// <summary>Update a meeting's details. Creator or Admin/SuperAdmin only.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<MeetingDto>>> Update(
        int id, [FromBody] UpdateMeetingRequestDto request, CancellationToken ct)
    {
        var meeting = await _meetings.UpdateMeetingAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingDto>.Ok(meeting, "Meeting updated."));
    }

    /// <summary>
    /// Update only the status of a meeting (Scheduled → InProgress → Completed / Cancelled).
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<MeetingDto>>> UpdateStatus(
        int id, [FromBody] UpdateMeetingStatusRequestDto request, CancellationToken ct)
    {
        var meeting = await _meetings.UpdateStatusAsync(id, request.Status, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingDto>.Ok(meeting, $"Meeting status updated to '{request.Status}'."));
    }

    /// <summary>Delete a meeting permanently. SuperAdmin/Admin or the creator.</summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken ct)
    {
        await _meetings.DeleteMeetingAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<object?>.Ok(null, "Meeting deleted."));
    }

    // ============================================================ Participants

    /// <summary>Get the participant list for a meeting.</summary>
    [HttpGet("{id:int}/participants")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MeetingParticipantDto>>>> GetParticipants(
        int id, CancellationToken ct)
    {
        var parts = await _meetings.GetParticipantsAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<MeetingParticipantDto>>.Ok(parts));
    }

    /// <summary>Add a user or client as a participant. Write access required.</summary>
    [HttpPost("{id:int}/participants")]
    public async Task<ActionResult<ApiResponse<MeetingParticipantDto>>> AddParticipant(
        int id, [FromBody] AddMeetingParticipantRequestDto request, CancellationToken ct)
    {
        var part = await _meetings.AddParticipantAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingParticipantDto>.Ok(part, "Participant added."));
    }

    /// <summary>Remove a participant from a meeting. Creator / Admin / SuperAdmin.</summary>
    [HttpDelete("{id:int}/participants/{participantId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveParticipant(
        int id, int participantId, CancellationToken ct)
    {
        await _meetings.RemoveParticipantAsync(id, participantId, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<object?>.Ok(null, "Participant removed."));
    }

    /// <summary>
    /// Current user responds to their own meeting invitation.
    /// Status: Accepted | Declined | Tentative
    /// </summary>
    [HttpPost("{id:int}/respond")]
    public async Task<ActionResult<ApiResponse<object>>> Respond(
        int id, [FromBody] RespondMeetingRequestDto request, CancellationToken ct)
    {
        await _meetings.RespondAsync(id, request, CurrentUserId, ct);
        return Ok(ApiResponse<object?>.Ok(null, $"Response recorded: {request.Status}."));
    }

    // ============================================================ Payments

    /// <summary>
    /// Get all payment records for a meeting.
    /// SuperAdmin / Admin / Manager only.
    /// </summary>
    [HttpGet("{id:int}/payments")]
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MeetingPaymentDto>>>> GetPayments(
        int id, CancellationToken ct)
    {
        var payments = await _meetings.GetPaymentsAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<MeetingPaymentDto>>.Ok(payments));
    }

    /// <summary>
    /// Get the payment summary (paid vs outstanding) for a meeting.
    /// SuperAdmin / Admin / Manager only.
    /// </summary>
    [HttpGet("{id:int}/payments/summary")]
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    public async Task<ActionResult<ApiResponse<MeetingPaymentSummaryDto>>> GetPaymentSummary(
        int id, CancellationToken ct)
    {
        var summary = await _meetings.GetPaymentSummaryAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingPaymentSummaryDto>.Ok(summary));
    }

    /// <summary>
    /// Record a payment for a participant slot (cash or online).
    /// SuperAdmin / Admin only.
    /// </summary>
    [HttpPost("{id:int}/payments")]
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    public async Task<ActionResult<ApiResponse<MeetingPaymentDto>>> RecordPayment(
        int id, [FromBody] RecordMeetingPaymentRequestDto request, CancellationToken ct)
    {
        var payment = await _meetings.RecordPaymentAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<MeetingPaymentDto>.Ok(payment, "Payment recorded."));
    }
}
