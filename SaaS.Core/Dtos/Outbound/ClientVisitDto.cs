/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>A scheduled visit as shown to a client / in the client detail.</summary>
public class ClientVisitDto
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ClientId { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? ServiceTypeName { get; set; }
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public string? AssignedUserAvatarUrl { get; set; }
    public string? Location { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public long WorkedSeconds { get; set; }
    public bool HasClockOutSignature { get; set; }
}

/// <summary>A single visit with its time entries and signatures, for the portal detail.</summary>
public sealed class ClientVisitDetailDto : ClientVisitDto
{
    public IReadOnlyList<TimeEntryDto> TimeEntries { get; set; } = new List<TimeEntryDto>();
    public IReadOnlyList<TimeEntrySignatureDto> Signatures { get; set; } = new List<TimeEntrySignatureDto>();
}
