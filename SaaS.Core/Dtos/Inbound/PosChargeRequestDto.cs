using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class PosChargeRequestDto
{
    [Required, MaxLength(200)]
    public string PayerName { get; set; } = string.Empty;

    [EmailAddress]
    public string? PayerEmail { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required, Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [Required]
    public string CardNumber { get; set; } = string.Empty;
}

public sealed class UpdatePosFeeSettingsDto
{
    [Range(0, 100)]
    public decimal FeePercent { get; set; }

    [Range(0, 10_000)]
    public decimal FeeFixed { get; set; }
}
