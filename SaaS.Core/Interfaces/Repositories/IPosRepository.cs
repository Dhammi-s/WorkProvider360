/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IPosRepository
{
    Task<Guid> CreateAsync(PosTransaction txn, CancellationToken ct = default);
    Task<IReadOnlyList<PosTransaction>> GetAllAsync(CancellationToken ct = default);
    Task<PosFeeSettings?> GetFeeSettingsAsync(CancellationToken ct = default);
    Task<PosFeeSettings> UpsertFeeSettingsAsync(PosFeeSettings settings, CancellationToken ct = default);
}
