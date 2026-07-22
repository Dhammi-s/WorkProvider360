using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Default pay rate / overtime and notification defaults for new schedules.</summary>
public sealed class UpdateSchedulingDefaultsDto
{
    [Range(0, 100000)]
    public decimal DefaultPayRatePerHour { get; set; }

    [Range(1, 10)]
    public decimal DefaultOvertimeMultiplier { get; set; } = 1.5m;

    public bool NotifyAdminOnCreate { get; set; }

    public bool NotifyManagerOnCreate { get; set; }
}
