/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface IPosService
{
    Task<PosTransactionDto> ChargeAsync(PosChargeRequestDto request, int currentUserId, CancellationToken ct = default);
    Task<IReadOnlyList<PosTransactionDto>> GetTransactionsAsync(CancellationToken ct = default);
    Task<PosSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    Task<PosFeeSettingsDto> GetFeeSettingsAsync(CancellationToken ct = default);
    Task<PosFeeSettingsDto> UpdateFeeSettingsAsync(UpdatePosFeeSettingsDto request, CancellationToken ct = default);
}
