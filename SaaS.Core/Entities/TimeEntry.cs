namespace SaaS.Core.Entities;

/// <summary>
/// A worked-time record for a schedule. Source is "Timer" (clock in/out) or
/// "Manual". ClockOutUtc is null while a timer is still running.
/// </summary>
public sealed class TimeEntry
{
    public int TimeEntryId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public string Source { get; set; } = "Timer";
    public string? Note { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
