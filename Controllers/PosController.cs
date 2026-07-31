using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Point-of-sale (sandbox). SuperAdmin / Admin can run card sales and view the
/// earnings ledger; only SuperAdmin sets the platform fee.
/// </summary>
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
public sealed class PosController : BaseApiController
{
    private readonly IPosService _pos;

    public PosController(IPosService pos) => _pos = pos;

    [HttpPost("charge")]
    public async Task<ActionResult<ApiResponse<PosTransactionDto>>> Charge(
        [FromBody] PosChargeRequestDto request, CancellationToken ct)
    {
        var txn = await _pos.ChargeAsync(request, CurrentUserId, ct);
        var message = txn.Status == "Approved"
            ? $"Approved. Platform earned {txn.PlatformFee:C2}."
            : $"Declined: {txn.DeclineReason}";
        return Ok(ApiResponse<PosTransactionDto>.Ok(txn, message));
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PosTransactionDto>>>> Transactions(CancellationToken ct)
    {
        var txns = await _pos.GetTransactionsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<PosTransactionDto>>.Ok(txns));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<PosSummaryDto>>> Summary(CancellationToken ct)
    {
        var summary = await _pos.GetSummaryAsync(ct);
        return Ok(ApiResponse<PosSummaryDto>.Ok(summary));
    }

    [HttpGet("fee-settings")]
    public async Task<ActionResult<ApiResponse<PosFeeSettingsDto>>> GetFeeSettings(CancellationToken ct)
    {
        var settings = await _pos.GetFeeSettingsAsync(ct);
        return Ok(ApiResponse<PosFeeSettingsDto>.Ok(settings));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("fee-settings")]
    public async Task<ActionResult<ApiResponse<PosFeeSettingsDto>>> UpdateFeeSettings(
        [FromBody] UpdatePosFeeSettingsDto request, CancellationToken ct)
    {
        var settings = await _pos.UpdateFeeSettingsAsync(request, ct);
        return Ok(ApiResponse<PosFeeSettingsDto>.Ok(settings, "Fee settings saved."));
    }
}
