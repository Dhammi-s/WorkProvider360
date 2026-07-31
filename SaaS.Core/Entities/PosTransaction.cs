namespace SaaS.Core.Entities;

/// <summary>A point-of-sale card sale: a customer paying an agency; the platform earns a fee.</summary>
public sealed class PosTransaction
{
    public Guid PosTransactionId { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public string? PayerEmail { get; set; }
    public string? Description { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal FeePercent { get; set; }
    public decimal FeeFixed { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public string? CardLast4 { get; set; }
    public string Status { get; set; } = string.Empty; // Approved | Declined
    public string? DeclineReason { get; set; }
    public string Provider { get; set; } = string.Empty; // Mock | Stripe | Bank...
    public string? ProviderRef { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>Single-row (SettingsId = 1) platform fee configuration.</summary>
public sealed class PosFeeSettings
{
    public int SettingsId { get; set; }
    public decimal FeePercent { get; set; }
    public decimal FeeFixed { get; set; }
    public DateTime UpdatedOn { get; set; }
}
