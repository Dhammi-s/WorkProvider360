/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// An office meeting scheduled between users, clients, admin, managers or
/// super-admin. Times are stored in UTC.
/// MeetingType : "InPerson" | "Online" | "Hybrid"
/// Status      : "Scheduled" | "InProgress" | "Completed" | "Cancelled"
/// </summary>
public sealed class Meeting
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    /// <summary>Physical address or video-call URL.</summary>
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

    /// <summary>Populated by usp_Meeting_GetAll / GetById.</summary>
    public int ParticipantCount { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
