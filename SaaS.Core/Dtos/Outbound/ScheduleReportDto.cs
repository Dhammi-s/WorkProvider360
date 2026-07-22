namespace SaaS.Core.Dtos.Outbound;

/// <summary>Aggregated hours + earnings for one user over the report period.</summary>
public sealed class ScheduleReportRowDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ScheduleCount { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public decimal RegularPay { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal TotalPay { get; set; }
}

/// <summary>Report response: per-user rows plus grand totals for the period.</summary>
public sealed class ScheduleReportDto
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public IReadOnlyList<ScheduleReportRowDto> Rows { get; set; } = new List<ScheduleReportRowDto>();
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalPay { get; set; }
}
