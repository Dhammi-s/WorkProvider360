namespace SaaS.Core.Dtos.Outbound;

public sealed class PosTransactionDto
{
    public Guid PosTransactionId { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string? PayerEmail { get; set; }
    public string? Description { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public string? CardLast4 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DeclineReason { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

public sealed class PosFeeSettingsDto
{
    public decimal FeePercent { get; set; }
    public decimal FeeFixed { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>Earnings ledger totals (approved sales only).</summary>
public sealed class PosSummaryDto
{
    public int ApprovedCount { get; set; }
    public int DeclinedCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalPlatformFees { get; set; } // what the platform earned
    public decimal TotalNet { get; set; }          // what merchants kept
}
