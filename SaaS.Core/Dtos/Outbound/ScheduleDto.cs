namespace SaaS.Core.Dtos.Outbound;

/// <summary>Outward-facing schedule. Times are UTC (ISO-8601 on the wire).</summary>
public sealed class ScheduleDto
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Location { get; set; }
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal PayRatePerHour { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? ColorTag { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>Schedule with its notes and time entries, for the detail view.</summary>
public sealed class ScheduleDetailDto
{
    public ScheduleDto Schedule { get; set; } = new();
    public IReadOnlyList<ScheduleNoteDto> Notes { get; set; } = new List<ScheduleNoteDto>();
    public IReadOnlyList<TimeEntryDto> TimeEntries { get; set; } = new List<TimeEntryDto>();
}
