using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Invoices / payroll. SuperAdmin records payments (cash in Phase 1), which stores
/// the browser-generated PDF and emails it to the recipient, and lists paid invoices.
/// </summary>
[Authorize(Roles = RoleConstants.SuperAdmin)]
public sealed class InvoicesController : BaseApiController
{
    private readonly IInvoiceService _invoices;

    public InvoicesController(IInvoiceService invoices) => _invoices = invoices;

    /// <summary>Creates a Stripe Checkout session for an online payment.</summary>
    [HttpPost("checkout-session")]
    public async Task<ActionResult<ApiResponse<CheckoutSessionDto>>> CreateCheckout(
        [FromBody] CheckoutRequestDto request, CancellationToken ct)
    {
        var session = await _invoices.CreateCheckoutSessionAsync(request, ct);
        return Ok(ApiResponse<CheckoutSessionDto>.Ok(session));
    }

    [HttpPost("pay")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> Pay(
        [FromBody] PayInvoiceRequestDto request, CancellationToken ct)
    {
        var invoice = await _invoices.PayAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse<InvoiceDto>.Ok(invoice, $"Paid. Invoice {invoice.InvoiceNumber} emailed to {invoice.RecipientEmail}."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvoiceDto>>>> GetAll(CancellationToken ct)
    {
        var invoices = await _invoices.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<InvoiceDto>>.Ok(invoices));
    }

    /// <summary>Returns the stored PDF (base64 data URI) for re-download.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<ActionResult<ApiResponse<string>>> GetPdf(Guid id, CancellationToken ct)
    {
        var pdf = await _invoices.GetPdfAsync(id, ct);
        return pdf is null
            ? NotFound(ApiResponse.Fail("Invoice not found."))
            : Ok(ApiResponse<string>.Ok(pdf));
    }
}
