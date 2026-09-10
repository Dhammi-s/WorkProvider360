/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

// ------------------------------------------------------------------ Settings

public sealed class UpdateMeetingSettingsRequestDto
{
    [Required]
    public string AdminAccess { get; set; } = "Write";          // None | Read | Write

    [Required]
    public string ManagerAccess { get; set; } = "Write";        // None | Read | Write

    public bool UserCanCreate { get; set; }
    public bool AllowClientParticipants { get; set; } = true;
    public bool AllowPaidMeetings { get; set; }

    [Range(0, 99999)]
    public decimal DefaultFeePerParticipant { get; set; }

    public bool RequireApproval { get; set; }
    public bool NotifyOnCreate { get; set; } = true;
    public bool NotifyOnUpdate { get; set; }
    public bool NotifyOnCancel { get; set; } = true;

    [Range(1, 10000)]
    public int MaxParticipantsDefault { get; set; } = 50;
}

// ------------------------------------------------------------------ Meetings

public sealed class CreateMeetingRequestDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    /// <summary>InPerson | Online | Hybrid</summary>
    public string MeetingType { get; set; } = "InPerson";

    public bool IsPaid { get; set; }

    [Range(0, 99999)]
    public decimal? FeePerParticipant { get; set; }

    [Range(1, 10000)]
    public int? MaxParticipants { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string? ColorTag { get; set; }

    /// <summary>Optional: participant user IDs to invite immediately.</summary>
    public List<int>? ParticipantUserIds { get; set; }

    /// <summary>Optional: participant client IDs to invite immediately.</summary>
    public List<int>? ParticipantClientIds { get; set; }
}

public sealed class UpdateMeetingRequestDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    public string MeetingType { get; set; } = "InPerson";
    public bool IsPaid { get; set; }

    [Range(0, 99999)]
    public decimal? FeePerParticipant { get; set; }

    [Range(1, 10000)]
    public int? MaxParticipants { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string? ColorTag { get; set; }
}

public sealed class UpdateMeetingStatusRequestDto
{
    /// <summary>Scheduled | InProgress | Completed | Cancelled</summary>
    [Required]
    public string Status { get; set; } = string.Empty;
}

// ------------------------------------------------------------ Participants

public sealed class AddMeetingParticipantRequestDto
{
    /// <summary>Set if inviting a user (by UserId).</summary>
    public int? UserId { get; set; }

    /// <summary>Set if inviting a client (by ClientId).</summary>
    public int? ClientId { get; set; }

    /// <summary>Host | Attendee | Optional</summary>
    public string ParticipantRole { get; set; } = "Attendee";
}

public sealed class RespondMeetingRequestDto
{
    /// <summary>Accepted | Declined | Tentative</summary>
    [Required]
    public string Status { get; set; } = string.Empty;
}

// -------------------------------------------------------------- Payments

public sealed class RecordMeetingPaymentRequestDto
{
    [Required]
    public int ParticipantId { get; set; }

    [Required, Range(0.01, 99999)]
    public decimal Amount { get; set; }

    /// <summary>Cash | Online</summary>
    [Required]
    public string Method { get; set; } = "Cash";

    /// <summary>Pending | Paid | Refunded</summary>
    public string Status { get; set; } = "Paid";

    [MaxLength(200)]
    public string? TransactionId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
