using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Point-of-sale sandbox: charges a customer's card via the pluggable payment
/// provider and records the platform fee the business earns on each sale.
/// </summary>
public sealed class PosService : IPosService
{
    private const decimal DefaultFeePercent = 1.00m;
    private const decimal DefaultFeeFixed = 0.20m;

    private readonly IPosRepository _pos;
    private readonly IPaymentProvider _provider;

    public PosService(IPosRepository pos, IPaymentProvider provider)
    {
        _pos = pos;
        _provider = provider;
    }

    public async Task<PosTransactionDto> ChargeAsync(PosChargeRequestDto request, int currentUserId, CancellationToken ct = default)
    {
        var settings = await _pos.GetFeeSettingsAsync(ct);
        var feePercent = settings?.FeePercent ?? DefaultFeePercent;
        var feeFixed = settings?.FeeFixed ?? DefaultFeeFixed;

        var result = await _provider.AuthorizeAndCaptureAsync(new PaymentChargeRequest
        {
            Amount = request.Amount,
            CardNumber = request.CardNumber,
            Description = request.Description,
        }, ct);

        decimal platformFee = 0m, net = 0m;
        if (result.Success)
        {
            platformFee = Round(request.Amount * (feePercent / 100m) + feeFixed);
            if (platformFee > request.Amount) platformFee = request.Amount;
            net = Round(request.Amount - platformFee);
        }

        var txn = new PosTransaction
        {
            PayerName = request.PayerName,
            PayerEmail = request.PayerEmail,
            Description = request.Description,
            GrossAmount = request.Amount,
            FeePercent = feePercent,
            FeeFixed = feeFixed,
            PlatformFee = platformFee,
            NetAmount = net,
            CardLast4 = result.CardLast4,
            Status = result.Success ? "Approved" : "Declined",
            DeclineReason = result.DeclineReason,
            Provider = result.Provider,
            ProviderRef = result.ProviderRef,
            CreatedByUserId = currentUserId,
        };
        txn.PosTransactionId = await _pos.CreateAsync(txn, ct);
        txn.CreatedOn = DateTime.UtcNow;

        return Map(txn);
    }

    public async Task<IReadOnlyList<PosTransactionDto>> GetTransactionsAsync(CancellationToken ct = default)
    {
        var rows = await _pos.GetAllAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<PosSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var rows = await _pos.GetAllAsync(ct);
        var approved = rows.Where(r => r.Status == "Approved").ToList();
        return new PosSummaryDto
        {
            ApprovedCount = approved.Count,
            DeclinedCount = rows.Count(r => r.Status == "Declined"),
            TotalGross = approved.Sum(r => r.GrossAmount),
            TotalPlatformFees = approved.Sum(r => r.PlatformFee),
            TotalNet = approved.Sum(r => r.NetAmount),
        };
    }

    public async Task<PosFeeSettingsDto> GetFeeSettingsAsync(CancellationToken ct = default)
    {
        var s = await _pos.GetFeeSettingsAsync(ct);
        return new PosFeeSettingsDto
        {
            FeePercent = s?.FeePercent ?? DefaultFeePercent,
            FeeFixed = s?.FeeFixed ?? DefaultFeeFixed,
            UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
        };
    }

    public async Task<PosFeeSettingsDto> UpdateFeeSettingsAsync(UpdatePosFeeSettingsDto request, CancellationToken ct = default)
    {
        var saved = await _pos.UpsertFeeSettingsAsync(new PosFeeSettings
        {
            FeePercent = request.FeePercent,
            FeeFixed = request.FeeFixed,
        }, ct);
        return new PosFeeSettingsDto { FeePercent = saved.FeePercent, FeeFixed = saved.FeeFixed, UpdatedOn = saved.UpdatedOn };
    }

    private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static PosTransactionDto Map(PosTransaction t) => new()
    {
        PosTransactionId = t.PosTransactionId,
        PayerName = t.PayerName,
        PayerEmail = t.PayerEmail,
        Description = t.Description,
        GrossAmount = t.GrossAmount,
        PlatformFee = t.PlatformFee,
        NetAmount = t.NetAmount,
        CardLast4 = t.CardLast4,
        Status = t.Status,
        DeclineReason = t.DeclineReason,
        Provider = t.Provider,
        CreatedOn = t.CreatedOn,
    };
}
