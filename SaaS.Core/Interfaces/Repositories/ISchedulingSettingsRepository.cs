using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface ISchedulingSettingsRepository
{
    Task<SchedulingSettings?> GetAsync(CancellationToken ct = default);
    Task<SchedulingSettings> UpdateAccessAsync(string adminAccess, string managerAccess, CancellationToken ct = default);
    Task<SchedulingSettings> UpdateDefaultsAsync(decimal defaultPayRatePerHour, decimal defaultOvertimeMultiplier, bool notifyAdminOnCreate, bool notifyManagerOnCreate, bool autoClockEnabled, CancellationToken ct = default);
}
