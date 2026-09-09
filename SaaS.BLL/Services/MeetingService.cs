/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Business logic for the office meeting scheduling feature.
/// Access rules mirror the scheduling module:
///   SuperAdmin  → always full-write
///   Admin       → governed by MeetingSettings.AdminAccess
///   Manager     → governed by MeetingSettings.ManagerAccess
///   User        → can view own meetings; create only if UserCanCreate = true
///   Client      → can view their own meeting invitations only
/// </summary>
public sealed class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _meetings;
    private readonly IEmailService _email;

    public MeetingService(IMeetingRepository meetings, IEmailService email)
    {
        _meetings = meetings;
        _email    = email;
    }

    // ================================================================= Settings

    public async Task<MeetingSettingsDto> GetSettingsAsync(int callerRoleId, CancellationToken ct = default)
    {
        var s = await _meetings.GetSettingsAsync(ct);
        return Map(s);
    }

    public async Task<MeetingSettingsDto> UpdateSettingsAsync(
        UpdateMeetingSettingsRequestDto request, int callerRoleId, CancellationToken ct = default)
    {
        if (callerRoleId != RoleConstants.SuperAdminId)
            throw new UnauthorizedAccessException("Only SuperAdmin can change meeting settings.");

        ValidateAccessLevel(request.AdminAccess, nameof(request.AdminAccess));
        ValidateAccessLevel(request.ManagerAccess, nameof(request.ManagerAccess));

        var entity = new MeetingSettings
        {
            AdminAccess              = request.AdminAccess,
            ManagerAccess            = request.ManagerAccess,
            UserCanCreate            = request.UserCanCreate,
            AllowClientParticipants  = request.AllowClientParticipants,
            AllowPaidMeetings        = request.AllowPaidMeetings,
            DefaultFeePerParticipant = request.DefaultFeePerParticipant,
            RequireApproval          = request.RequireApproval,
            NotifyOnCreate           = request.NotifyOnCreate,
            NotifyOnUpdate           = request.NotifyOnUpdate,
            NotifyOnCancel           = request.NotifyOnCancel,
            MaxParticipantsDefault   = request.MaxParticipantsDefault
        };

        var saved = await _meetings.UpdateSettingsAsync(entity, ct);
        return Map(saved);
    }

    public async Task<MeetingAccessDto> GetAccessAsync(int callerRoleId, CancellationToken ct = default)
    {
        var s    = await _meetings.GetSettingsAsync(ct);
        var (access, canCreate) = ResolveAccess(callerRoleId, s);

        return new MeetingAccessDto
        {
            Access              = access,
            CanCreate           = canCreate,
            CanEdit             = access == "Write",
            CanDelete           = callerRoleId is RoleConstants.SuperAdminId or RoleConstants.AdminId,
            CanManagePayments   = callerRoleId is RoleConstants.SuperAdminId or RoleConstants.AdminId,
            CanViewAll          = callerRoleId is RoleConstants.SuperAdminId or RoleConstants.AdminId or RoleConstants.ManagerId
        };
    }

    // ================================================================= Meetings

    public async Task<MeetingDto> CreateMeetingAsync(
        CreateMeetingRequestDto request, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, canCreate) = ResolveAccess(callerRoleId, settings);

        if (!canCreate)
            throw new UnauthorizedAccessException("You do not have permission to create meetings.");

        if (request.IsPaid && !settings.AllowPaidMeetings)
            throw new InvalidOperationException("Paid meetings are not enabled in settings.");

        if (request.EndUtc <= request.StartUtc)
            throw new ArgumentException("EndUtc must be after StartUtc.");

        ValidateMeetingType(request.MeetingType);

        // If approval is required and caller is not SuperAdmin, set to Scheduled (pending approval could be a separate status)
        // We keep Status="Scheduled" regardless; RequireApproval info is visible in settings.

        var meeting = new Meeting
        {
            Title             = request.Title,
            Description       = request.Description,
            StartUtc          = request.StartUtc,
            EndUtc            = request.EndUtc,
            Location          = request.Location,
            MeetingType       = request.MeetingType,
            IsPaid            = request.IsPaid,
            FeePerParticipant = request.IsPaid ? request.FeePerParticipant : null,
            CreatedByUserId   = callerUserId,
            MaxParticipants   = request.MaxParticipants ?? settings.MaxParticipantsDefault,
            Notes             = request.Notes,
            ColorTag          = request.ColorTag
        };

        var meetingId = await _meetings.CreateAsync(meeting, ct);

        // Bulk-invite any extra participants specified at creation time
        if (request.ParticipantUserIds is { Count: > 0 })
        {
            foreach (var uid in request.ParticipantUserIds.Where(uid => uid != callerUserId))
                await _meetings.AddParticipantAsync(meetingId, uid, null, "Attendee", ct);
        }

        if (request.ParticipantClientIds is { Count: > 0 })
        {
            if (!settings.AllowClientParticipants)
                throw new InvalidOperationException("Client participants are not enabled in settings.");

            foreach (var cid in request.ParticipantClientIds)
                await _meetings.AddParticipantAsync(meetingId, null, cid, "Attendee", ct);
        }

        var created = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new InvalidOperationException("Meeting was created but could not be retrieved.");

        // Send invitations to all participants (except the creator/host).
        // Emails are best-effort — failures never block the API response.
        if (request.ParticipantUserIds is { Count: > 0 } || request.ParticipantClientIds is { Count: > 0 })
        {
            var allParts = await _meetings.GetParticipantsAsync(meetingId, ct);
            foreach (var p in allParts.Where(p => p.ParticipantRole != "Host"))
                await SendInviteEmailAsync(p, created, ct);
        }

        return Map(created);
    }

    public async Task<IReadOnlyList<MeetingDto>> GetMeetingsAsync(
        DateTime? from, DateTime? to, string? status,
        int? userId, int? clientId,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);

        if (access == "None")
            return Array.Empty<MeetingDto>();

        // Users and Clients can only see meetings they are participating in
        int? participantUserId   = null;
        int? participantClientId = null;

        if (callerRoleId == RoleConstants.UserId)
        {
            participantUserId = callerUserId;
        }
        else if (callerRoleId == RoleConstants.ClientId)
        {
            // Client role: clientId is stored as UserId in JWT (the linked client record).
            // We filter by participantClientId using the caller's user context.
            // Frontend must pass clientId query param for this case.
            participantClientId = clientId;
        }
        else
        {
            // Admin/Manager/SuperAdmin can filter optionally
            participantUserId   = userId;
            participantClientId = clientId;
        }

        var rows = await _meetings.GetAllAsync(from, to, status,
            null, participantUserId, participantClientId, ct);

        return rows.Select(Map).ToList();
    }

    public async Task<MeetingDetailDto> GetMeetingByIdAsync(
        int meetingId, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);

        if (access == "None")
            throw new UnauthorizedAccessException("You do not have access to meetings.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        // Users can only view meetings they are part of
        if (callerRoleId == RoleConstants.UserId)
        {
            var participants = await _meetings.GetParticipantsAsync(meetingId, ct);
            if (!participants.Any(p => p.UserId == callerUserId))
                throw new UnauthorizedAccessException("You are not a participant of this meeting.");
        }

        var parts = await _meetings.GetParticipantsAsync(meetingId, ct);

        return new MeetingDetailDto
        {
            MeetingId         = meeting.MeetingId,
            Title             = meeting.Title,
            Description       = meeting.Description,
            StartUtc          = meeting.StartUtc,
            EndUtc            = meeting.EndUtc,
            Location          = meeting.Location,
            MeetingType       = meeting.MeetingType,
            Status            = meeting.Status,
            IsPaid            = meeting.IsPaid,
            FeePerParticipant = meeting.FeePerParticipant,
            CreatedByUserId   = meeting.CreatedByUserId,
            CreatedByName     = meeting.CreatedByName,
            MaxParticipants   = meeting.MaxParticipants,
            Notes             = meeting.Notes,
            ColorTag          = meeting.ColorTag,
            ParticipantCount  = meeting.ParticipantCount,
            CreatedOn         = meeting.CreatedOn,
            UpdatedOn         = meeting.UpdatedOn,
            Participants      = parts.Select(MapParticipant).ToList()
        };
    }

    public async Task<MeetingDto> UpdateMeetingAsync(
        int meetingId, UpdateMeetingRequestDto request,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);
        if (access != "Write")
            throw new UnauthorizedAccessException("You do not have permission to edit meetings.");

        var existing = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        // Non-SuperAdmin can only edit their own meetings
        if (callerRoleId != RoleConstants.SuperAdminId && existing.CreatedByUserId != callerUserId)
            throw new UnauthorizedAccessException("You can only edit meetings you created.");

        if (existing.Status is "Completed" or "Cancelled")
            throw new InvalidOperationException($"Cannot edit a meeting with status '{existing.Status}'.");

        if (request.IsPaid && !settings.AllowPaidMeetings)
            throw new InvalidOperationException("Paid meetings are not enabled in settings.");

        if (request.EndUtc <= request.StartUtc)
            throw new ArgumentException("EndUtc must be after StartUtc.");

        ValidateMeetingType(request.MeetingType);

        existing.Title             = request.Title;
        existing.Description       = request.Description;
        existing.StartUtc          = request.StartUtc;
        existing.EndUtc            = request.EndUtc;
        existing.Location          = request.Location;
        existing.MeetingType       = request.MeetingType;
        existing.IsPaid            = request.IsPaid;
        existing.FeePerParticipant = request.IsPaid ? request.FeePerParticipant : null;
        existing.MaxParticipants   = request.MaxParticipants;
        existing.Notes             = request.Notes;
        existing.ColorTag          = request.ColorTag;

        await _meetings.UpdateAsync(existing, ct);

        var updated = await _meetings.GetByIdAsync(meetingId, ct)!;
        return Map(updated!);
    }

    public async Task<MeetingDto> UpdateStatusAsync(
        int meetingId, string status,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        ValidateStatus(status);

        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);
        if (access != "Write")
            throw new UnauthorizedAccessException("You do not have permission to update meeting status.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        // Non-SuperAdmin can only update status of their own meetings
        if (callerRoleId != RoleConstants.SuperAdminId && meeting.CreatedByUserId != callerUserId)
            throw new UnauthorizedAccessException("You can only update status of meetings you created.");

        await _meetings.UpdateStatusAsync(meetingId, status, ct);

        var updated = await _meetings.GetByIdAsync(meetingId, ct)!;
        return Map(updated!);
    }

    public async Task DeleteMeetingAsync(
        int meetingId, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        // Only SuperAdmin and Admin can delete; creator can also delete their own
        if (callerRoleId is not (RoleConstants.SuperAdminId or RoleConstants.AdminId))
        {
            var meeting = await _meetings.GetByIdAsync(meetingId, ct)
                ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");
            if (meeting.CreatedByUserId != callerUserId)
                throw new UnauthorizedAccessException("You do not have permission to delete this meeting.");
        }

        await _meetings.DeleteAsync(meetingId, ct);
    }

    // ================================================================= Participants

    public async Task<MeetingParticipantDto> AddParticipantAsync(
        int meetingId, AddMeetingParticipantRequestDto request,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        if (request.UserId is null && request.ClientId is null)
            throw new ArgumentException("Either UserId or ClientId must be provided.");

        if (request.UserId is not null && request.ClientId is not null)
            throw new ArgumentException("Provide only UserId or ClientId, not both.");

        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);
        if (access != "Write")
            throw new UnauthorizedAccessException("You do not have permission to add participants.");

        if (request.ClientId.HasValue && !settings.AllowClientParticipants)
            throw new InvalidOperationException("Client participants are not enabled in settings.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        if (meeting.Status is "Completed" or "Cancelled")
            throw new InvalidOperationException("Cannot add participants to a completed or cancelled meeting.");

        // Capacity check
        if (meeting.MaxParticipants.HasValue && meeting.ParticipantCount >= meeting.MaxParticipants)
            throw new InvalidOperationException($"Meeting has reached its maximum capacity of {meeting.MaxParticipants} participants.");

        ValidateParticipantRole(request.ParticipantRole);

        var participantId = await _meetings.AddParticipantAsync(
            meetingId, request.UserId, request.ClientId, request.ParticipantRole, ct);

        if (participantId == -1)
            throw new InvalidOperationException("This participant is already added to the meeting.");

        var parts = await _meetings.GetParticipantsAsync(meetingId, ct);
        var added = parts.FirstOrDefault(p => p.ParticipantId == participantId)
            ?? throw new InvalidOperationException("Participant was added but could not be retrieved.");

        // Notify the newly added participant — best-effort, never blocks the response.
        await SendInviteEmailAsync(added, meeting, ct);

        return MapParticipant(added);
    }

    public async Task<IReadOnlyList<MeetingParticipantDto>> GetParticipantsAsync(
        int meetingId, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);
        if (access == "None")
            throw new UnauthorizedAccessException("You do not have access to meetings.");

        // Ensure meeting exists
        _ = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        var parts = await _meetings.GetParticipantsAsync(meetingId, ct);
        return parts.Select(MapParticipant).ToList();
    }

    public async Task RemoveParticipantAsync(
        int meetingId, int participantId,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        var settings = await _meetings.GetSettingsAsync(ct);
        var (access, _) = ResolveAccess(callerRoleId, settings);
        if (access != "Write")
            throw new UnauthorizedAccessException("You do not have permission to remove participants.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        if (meeting.Status is "Completed" or "Cancelled")
            throw new InvalidOperationException("Cannot remove participants from a completed or cancelled meeting.");

        // Non-SuperAdmin can only manage their own meetings
        if (callerRoleId != RoleConstants.SuperAdminId && meeting.CreatedByUserId != callerUserId)
            throw new UnauthorizedAccessException("You can only manage participants for meetings you created.");

        var parts = await _meetings.GetParticipantsAsync(meetingId, ct);
        var part  = parts.FirstOrDefault(p => p.ParticipantId == participantId)
            ?? throw new KeyNotFoundException($"Participant {participantId} not found in meeting {meetingId}.");

        if (part.ParticipantRole == "Host")
            throw new InvalidOperationException("The host cannot be removed from their own meeting.");

        await _meetings.RemoveParticipantAsync(participantId, ct);
    }

    public async Task<bool> RespondAsync(
        int meetingId, RespondMeetingRequestDto request,
        int callerUserId,
        CancellationToken ct = default)
    {
        ValidateRsvpStatus(request.Status);

        _ = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        var rowsAffected = await _meetings.RespondParticipantAsync(meetingId, callerUserId, request.Status, ct);

        if (rowsAffected == 0)
            throw new InvalidOperationException("You are not an invited participant of this meeting, or you are the host.");

        return true;
    }

    // ================================================================= Payments

    public async Task<MeetingPaymentDto> RecordPaymentAsync(
        int meetingId, RecordMeetingPaymentRequestDto request,
        int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        // Payment management is restricted to SuperAdmin and Admin
        if (callerRoleId is not (RoleConstants.SuperAdminId or RoleConstants.AdminId))
            throw new UnauthorizedAccessException("Only Admin or SuperAdmin can record meeting payments.");

        var settings = await _meetings.GetSettingsAsync(ct);
        if (!settings.AllowPaidMeetings)
            throw new InvalidOperationException("Paid meetings are not enabled in settings.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        if (!meeting.IsPaid)
            throw new InvalidOperationException("This meeting is not configured as a paid meeting.");

        ValidatePaymentMethod(request.Method);
        ValidatePaymentStatus(request.Status);

        var payment = new MeetingPayment
        {
            MeetingId        = meetingId,
            ParticipantId    = request.ParticipantId,
            Amount           = request.Amount,
            Method           = request.Method,
            Status           = request.Status,
            TransactionId    = request.TransactionId,
            Notes            = request.Notes,
            RecordedByUserId = callerUserId
        };

        var paymentId = await _meetings.RecordPaymentAsync(payment, ct);

        var payments = await _meetings.GetPaymentsAsync(meetingId, ct);
        var recorded = payments.FirstOrDefault(p => p.PaymentId == paymentId)
            ?? throw new InvalidOperationException("Payment was recorded but could not be retrieved.");

        return MapPayment(recorded);
    }

    public async Task<IReadOnlyList<MeetingPaymentDto>> GetPaymentsAsync(
        int meetingId, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        if (callerRoleId is not (RoleConstants.SuperAdminId or RoleConstants.AdminId or RoleConstants.ManagerId))
            throw new UnauthorizedAccessException("You do not have permission to view payment records.");

        _ = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        var payments = await _meetings.GetPaymentsAsync(meetingId, ct);
        return payments.Select(MapPayment).ToList();
    }

    public async Task<MeetingPaymentSummaryDto> GetPaymentSummaryAsync(
        int meetingId, int callerUserId, int callerRoleId,
        CancellationToken ct = default)
    {
        if (callerRoleId is not (RoleConstants.SuperAdminId or RoleConstants.AdminId or RoleConstants.ManagerId))
            throw new UnauthorizedAccessException("You do not have permission to view payment summary.");

        var meeting = await _meetings.GetByIdAsync(meetingId, ct)
            ?? throw new KeyNotFoundException($"Meeting {meetingId} not found.");

        var participants = await _meetings.GetParticipantsAsync(meetingId, ct);
        var payments     = await _meetings.GetPaymentsAsync(meetingId, ct);

        var feePerPart   = meeting.FeePerParticipant ?? 0m;
        var paid         = participants.Count(p => p.IsPaid);
        var unpaid       = participants.Count - paid;
        var collected    = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount);
        var expected     = feePerPart * participants.Count;

        return new MeetingPaymentSummaryDto
        {
            TotalParticipants  = participants.Count,
            PaidParticipants   = paid,
            UnpaidParticipants = unpaid,
            TotalExpected      = expected,
            TotalCollected     = collected,
            TotalOutstanding   = Math.Max(0, expected - collected)
        };
    }

    // ================================================================= Helpers

    private static (string access, bool canCreate) ResolveAccess(int roleId, MeetingSettings s)
        => roleId switch
        {
            RoleConstants.SuperAdminId => ("Write", true),
            RoleConstants.AdminId      => (s.AdminAccess,   s.AdminAccess   == "Write"),
            RoleConstants.ManagerId    => (s.ManagerAccess, s.ManagerAccess == "Write"),
            RoleConstants.UserId       => ("Read", s.UserCanCreate),
            RoleConstants.ClientId     => ("Read", false),
            _                          => ("None", false)
        };

    private static void ValidateAccessLevel(string value, string field)
    {
        if (value is not ("None" or "Read" or "Write"))
            throw new ArgumentException($"{field} must be 'None', 'Read', or 'Write'.");
    }

    private static void ValidateMeetingType(string value)
    {
        if (value is not ("InPerson" or "Online" or "Hybrid"))
            throw new ArgumentException("MeetingType must be 'InPerson', 'Online', or 'Hybrid'.");
    }

    private static void ValidateStatus(string value)
    {
        if (value is not ("Scheduled" or "InProgress" or "Completed" or "Cancelled"))
            throw new ArgumentException("Status must be 'Scheduled', 'InProgress', 'Completed', or 'Cancelled'.");
    }

    private static void ValidateRsvpStatus(string value)
    {
        if (value is not ("Accepted" or "Declined" or "Tentative"))
            throw new ArgumentException("Status must be 'Accepted', 'Declined', or 'Tentative'.");
    }

    private static void ValidateParticipantRole(string value)
    {
        if (value is not ("Host" or "Attendee" or "Optional"))
            throw new ArgumentException("ParticipantRole must be 'Host', 'Attendee', or 'Optional'.");
    }

    private static void ValidatePaymentMethod(string value)
    {
        if (value is not ("Cash" or "Online"))
            throw new ArgumentException("Payment method must be 'Cash' or 'Online'.");
    }

    private static void ValidatePaymentStatus(string value)
    {
        if (value is not ("Pending" or "Paid" or "Refunded"))
            throw new ArgumentException("Payment status must be 'Pending', 'Paid', or 'Refunded'.");
    }

    /// <summary>
    /// Best-effort meeting-invite email. Never throws — SMTP failures are
    /// caught here (and already logged inside EmailService).
    /// </summary>
    private async Task SendInviteEmailAsync(MeetingParticipant p, Meeting meeting, CancellationToken ct)
    {
        // Resolve the correct email and display name depending on whether
        // this is an internal user or an external client participant.
        var toAddress = p.UserId.HasValue
            ? p.ParticipantEmail
            : (p.ClientEmail ?? p.ParticipantEmail);

        var displayName = p.UserId.HasValue
            ? p.ParticipantName
            : (p.ClientName ?? p.ParticipantName);

        if (string.IsNullOrWhiteSpace(toAddress)) return;

        try
        {
            await _email.SendMeetingInviteAsync(
                toAddress,
                displayName ?? "Participant",
                meeting.Title,
                meeting.MeetingType,
                meeting.Location,
                meeting.StartUtc,
                meeting.EndUtc,
                meeting.CreatedByName ?? "Organizer",
                meeting.IsPaid,
                meeting.FeePerParticipant,
                ct);
        }
        catch
        {
            // Best-effort — EmailService already records the failure in the email log.
        }
    }

    // ================================================================= Mappers

    private static MeetingSettingsDto Map(MeetingSettings s) => new()
    {
        AdminAccess              = s.AdminAccess,
        ManagerAccess            = s.ManagerAccess,
        UserCanCreate            = s.UserCanCreate,
        AllowClientParticipants  = s.AllowClientParticipants,
        AllowPaidMeetings        = s.AllowPaidMeetings,
        DefaultFeePerParticipant = s.DefaultFeePerParticipant,
        RequireApproval          = s.RequireApproval,
        NotifyOnCreate           = s.NotifyOnCreate,
        NotifyOnUpdate           = s.NotifyOnUpdate,
        NotifyOnCancel           = s.NotifyOnCancel,
        MaxParticipantsDefault   = s.MaxParticipantsDefault,
        UpdatedOn                = s.UpdatedOn
    };

    private static MeetingDto Map(Meeting m) => new()
    {
        MeetingId         = m.MeetingId,
        Title             = m.Title,
        Description       = m.Description,
        StartUtc          = m.StartUtc,
        EndUtc            = m.EndUtc,
        Location          = m.Location,
        MeetingType       = m.MeetingType,
        Status            = m.Status,
        IsPaid            = m.IsPaid,
        FeePerParticipant = m.FeePerParticipant,
        CreatedByUserId   = m.CreatedByUserId,
        CreatedByName     = m.CreatedByName,
        MaxParticipants   = m.MaxParticipants,
        Notes             = m.Notes,
        ColorTag          = m.ColorTag,
        ParticipantCount  = m.ParticipantCount,
        CreatedOn         = m.CreatedOn,
        UpdatedOn         = m.UpdatedOn
    };

    private static MeetingParticipantDto MapParticipant(MeetingParticipant p) => new()
    {
        ParticipantId      = p.ParticipantId,
        MeetingId          = p.MeetingId,
        UserId             = p.UserId,
        ParticipantName    = p.ParticipantName,
        ParticipantEmail   = p.ParticipantEmail,
        ParticipantRoleName = p.ParticipantRoleName,
        ClientId           = p.ClientId,
        ClientName         = p.ClientName,
        ClientEmail        = p.ClientEmail,
        ParticipantRole    = p.ParticipantRole,
        Status             = p.Status,
        IsPaid             = p.IsPaid,
        PaymentAmount      = p.PaymentAmount,
        PaymentDate        = p.PaymentDate,
        PaymentMethod      = p.PaymentMethod,
        InvitedAt          = p.InvitedAt,
        RespondedAt        = p.RespondedAt
    };

    private static MeetingPaymentDto MapPayment(MeetingPayment p) => new()
    {
        PaymentId              = p.PaymentId,
        MeetingId              = p.MeetingId,
        ParticipantId          = p.ParticipantId,
        Amount                 = p.Amount,
        Method                 = p.Method,
        Status                 = p.Status,
        TransactionId          = p.TransactionId,
        Notes                  = p.Notes,
        PaidAt                 = p.PaidAt,
        RecordedByUserId       = p.RecordedByUserId,
        RecordedByName         = p.RecordedByName,
        ParticipantUserId      = p.ParticipantUserId,
        ParticipantName        = p.ParticipantName,
        ParticipantClientId    = p.ParticipantClientId,
        ParticipantClientName  = p.ParticipantClientName
    };
}
