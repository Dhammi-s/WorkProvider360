namespace SaaS.Core.Entities;

/// <summary>A paid invoice (salary to an admin/manager, or shift pay to a user).</summary>
public sealed class Invoice
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? RecipientUserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientRoleName { get; set; }
    public string InvoiceType { get; set; } = string.Empty; // Salary | ShiftPay
    public decimal Amount { get; set; }
    public decimal? RegularHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public decimal? TotalHours { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public string? Details { get; set; }
    public string? PdfBase64 { get; set; }
    public string Status { get; set; } = "Paid";
    public string PaymentMethod { get; set; } = "Cash"; // Cash | Online
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? PaidOn { get; set; }
}
