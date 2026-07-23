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
