/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

public sealed class InvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? RecipientUserId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientRoleName { get; set; }
    public string InvoiceType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? RegularHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public decimal? TotalHours { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }
    public string? Details { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? PaidOn { get; set; }
}
