/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

// ------------------------------------------------------------------ Settings

public sealed class MeetingSettingsDto
{
    public string AdminAccess { get; set; } = "Write";
    public string ManagerAccess { get; set; } = "Write";
    public bool UserCanCreate { get; set; }
    public bool AllowClientParticipants { get; set; }
    public bool AllowPaidMeetings { get; set; }
    public decimal DefaultFeePerParticipant { get; set; }
    public bool RequireApproval { get; set; }
    public bool NotifyOnCreate { get; set; }
    public bool NotifyOnUpdate { get; set; }
    public bool NotifyOnCancel { get; set; }
    public int MaxParticipantsDefault { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// The current user's effective access level for the meetings feature.
/// </summary>
public sealed class MeetingAccessDto
{
    /// <summary>None | Read | Write</summary>
    public string Access { get; set; } = "None";
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanManagePayments { get; set; }
    public bool CanViewAll { get; set; }
}

// ------------------------------------------------------------------ Meeting

public class MeetingDto
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? Location { get; set; }
    public string MeetingType { get; set; } = "InPerson";
    public string Status { get; set; } = "Scheduled";
    public bool IsPaid { get; set; }
    public decimal? FeePerParticipant { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public int? MaxParticipants { get; set; }
    public string? Notes { get; set; }
    public string? ColorTag { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

public sealed class MeetingDetailDto : MeetingDto
{
    public IReadOnlyList<MeetingParticipantDto> Participants { get; set; }
        = Array.Empty<MeetingParticipantDto>();
}

// ----------------------------------------------------------- Participant

public sealed class MeetingParticipantDto
{
    public int ParticipantId { get; set; }
    public int MeetingId { get; set; }

    public int? UserId { get; set; }
    public string? ParticipantName { get; set; }
    public string? ParticipantEmail { get; set; }
    public string? ParticipantRoleName { get; set; }

    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }

    /// <summary>Host | Attendee | Optional</summary>
    public string ParticipantRole { get; set; } = "Attendee";

    /// <summary>Pending | Accepted | Declined | Tentative</summary>
    public string Status { get; set; } = "Pending";

    public bool IsPaid { get; set; }
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

// ------------------------------------------------------------ Payment

public sealed class MeetingPaymentDto
{
    public int PaymentId { get; set; }
    public int MeetingId { get; set; }
    public int ParticipantId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
    public string Status { get; set; } = "Paid";
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
    public DateTime PaidAt { get; set; }
    public int RecordedByUserId { get; set; }
    public string? RecordedByName { get; set; }

    public int? ParticipantUserId { get; set; }
    public string? ParticipantName { get; set; }
    public int? ParticipantClientId { get; set; }
    public string? ParticipantClientName { get; set; }
}

public sealed class MeetingPaymentSummaryDto
{
    public int TotalParticipants { get; set; }
    public int PaidParticipants { get; set; }
    public int UnpaidParticipants { get; set; }
    public decimal TotalExpected { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
}
