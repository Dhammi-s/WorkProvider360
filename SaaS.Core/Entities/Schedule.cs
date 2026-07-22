namespace SaaS.Core.Entities;

/// <summary>
/// A scheduled job / shift assigned to a user. Times are stored in UTC.
/// </summary>
public sealed class Schedule
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Location { get; set; }
    public int AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal PayRatePerHour { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? RejectionReason { get; set; }
    public string? ColorTag { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
