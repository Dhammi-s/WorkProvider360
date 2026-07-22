namespace SaaS.Core.Dtos.Outbound;

public sealed class SchedulingSettingsDto
{
    public string AdminAccess { get; set; } = "Write";
    public string ManagerAccess { get; set; } = "Read";
    public decimal DefaultPayRatePerHour { get; set; }
    public decimal DefaultOvertimeMultiplier { get; set; } = 1.5m;
    public bool NotifyAdminOnCreate { get; set; }
    public bool NotifyManagerOnCreate { get; set; }
    public DateTime UpdatedOn { get; set; }
}
