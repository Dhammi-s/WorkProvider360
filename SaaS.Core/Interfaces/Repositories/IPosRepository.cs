using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IPosRepository
{
    Task<Guid> CreateAsync(PosTransaction txn, CancellationToken ct = default);
    Task<IReadOnlyList<PosTransaction>> GetAllAsync(CancellationToken ct = default);
    Task<PosFeeSettings?> GetFeeSettingsAsync(CancellationToken ct = default);
    Task<PosFeeSettings> UpsertFeeSettingsAsync(PosFeeSettings settings, CancellationToken ct = default);
}
