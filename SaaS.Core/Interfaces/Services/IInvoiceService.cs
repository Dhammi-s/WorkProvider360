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

/// <summary>Invoice / payment recording. SuperAdmin-only in the controller.</summary>
public interface IInvoiceService
{
    /// <summary>Creates a Stripe Checkout session for an online payment.</summary>
    Task<CheckoutSessionDto> CreateCheckoutSessionAsync(CheckoutRequestDto request, CancellationToken ct = default);

    /// <summary>Records a payment, stores the PDF, and emails it to the recipient.</summary>
    Task<InvoiceDto> PayAsync(PayInvoiceRequestDto request, int currentUserId, CancellationToken ct = default);

    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>The stored PDF (base64 data URI) for re-download, or null.</summary>
    Task<string?> GetPdfAsync(Guid invoiceId, CancellationToken ct = default);
}
