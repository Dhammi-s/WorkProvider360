/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A single attendee (user or client) on a meeting.
/// ParticipantRole : "Host" | "Attendee" | "Optional"
/// Status          : "Pending" | "Accepted" | "Declined" | "Tentative"
/// PaymentMethod   : "Cash" | "Online"
/// </summary>
public sealed class MeetingParticipant
{
    public int ParticipantId { get; set; }
    public int MeetingId { get; set; }

    // ----- User participant (role-based users)
    public int? UserId { get; set; }
    public string? ParticipantName { get; set; }
    public string? ParticipantEmail { get; set; }
    /// <summary>Role name of the user (SuperAdmin / Admin / Manager / User).</summary>
    public string? ParticipantRoleName { get; set; }

    // ----- Client participant
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }

    public string ParticipantRole { get; set; } = "Attendee";
    public string Status { get; set; } = "Pending";

    public bool IsPaid { get; set; }
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }

    public DateTime InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
