namespace SaaS.Core.Entities;

/// <summary>
/// Single-row tenant configuration for the scheduling feature: how much access
/// Admin/Manager roles get, and the pay defaults used when creating schedules.
/// AdminAccess / ManagerAccess are "None", "Read" or "Write".
/// </summary>
public sealed class SchedulingSettings
{
    public int SettingsId { get; set; }
    public string AdminAccess { get; set; } = "Write";
    public string ManagerAccess { get; set; } = "Read";
    public decimal DefaultPayRatePerHour { get; set; }
    public decimal DefaultOvertimeMultiplier { get; set; } = 1.5m;
    public bool NotifyAdminOnCreate { get; set; }
    public bool NotifyManagerOnCreate { get; set; }
    public bool AutoClockEnabled { get; set; }
    public DateTime UpdatedOn { get; set; }
}
