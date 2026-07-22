namespace SaaS.Core.Entities;

/// <summary>
/// A single schedule's raw report row as returned by usp_Schedule_GetReport.
/// The BLL layer derives regular/overtime hours and earnings from these values.
/// </summary>
public sealed class ScheduleReportRow
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal PayRatePerHour { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public string Status { get; set; } = string.Empty;
    public long WorkedSeconds { get; set; }
}
