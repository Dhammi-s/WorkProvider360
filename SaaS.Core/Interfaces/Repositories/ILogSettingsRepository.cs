using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface ILogSettingsRepository
{
    Task<LogSettings?> GetAsync(CancellationToken ct = default);
    Task<LogSettings> UpsertAsync(LogSettings settings, CancellationToken ct = default);
}
