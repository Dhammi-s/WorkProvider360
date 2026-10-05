/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A payment record for a paid meeting participant slot.
/// Method : "Cash" | "Online"
/// Status : "Pending" | "Paid" | "Refunded"
/// </summary>
public sealed class MeetingPayment
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

    // Participant info (joined in SP)
    public int? ParticipantUserId { get; set; }
    public string? ParticipantName { get; set; }
    public int? ParticipantClientId { get; set; }
    public string? ParticipantClientName { get; set; }
}
