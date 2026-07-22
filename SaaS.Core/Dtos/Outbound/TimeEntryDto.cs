namespace SaaS.Core.Dtos.Outbound;

public sealed class TimeEntryDto
{
    public int TimeEntryId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public string Source { get; set; } = "Timer";
    public string? Note { get; set; }

    /// <summary>Worked hours for this entry; 0 while the timer is still running.</summary>
    public decimal Hours { get; set; }
}
