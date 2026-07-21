using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IApplicationSettingsRepository
{
    Task<ApplicationSettings?> GetAsync(CancellationToken ct = default);
    Task<ApplicationSettings> UpsertAsync(ApplicationSettings settings, CancellationToken ct = default);
}
