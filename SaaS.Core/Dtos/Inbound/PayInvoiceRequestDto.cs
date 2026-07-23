using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Records a payment and its browser-generated PDF. The PDF (base64 data URI) is
/// stored and emailed as an attachment.
/// </summary>
public sealed class PayInvoiceRequestDto
{
    [Required, MaxLength(40)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public int? RecipientUserId { get; set; }

    [Required, MaxLength(200)]
    public string RecipientName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string RecipientEmail { get; set; } = string.Empty;

    public string? RecipientRoleName { get; set; }

    /// <summary>Salary | ShiftPay</summary>
    [Required]
    public string InvoiceType { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public decimal? RegularHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public decimal? TotalHours { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public string? Details { get; set; }

    /// <summary>Cash (Phase 1). Online is added in Phase 2 (Stripe).</summary>
    public string PaymentMethod { get; set; } = "Cash";

    /// <summary>Invoice PDF as a base64 data URI, generated in the browser.</summary>
    [Required]
    public string PdfBase64 { get; set; } = string.Empty;
}
