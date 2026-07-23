using System.Net;
using Microsoft.Extensions.Logging;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoices;
    private readonly IEmailService _email;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IInvoiceRepository invoices, IEmailService email, ILogger<InvoiceService> logger)
    {
        _invoices = invoices;
        _email = email;
        _logger = logger;
    }

    public async Task<InvoiceDto> PayAsync(PayInvoiceRequestDto request, int currentUserId, CancellationToken ct = default)
    {
        // Phase 1: cash only. Online (Stripe) arrives in Phase 2.
        var method = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Cash" : request.PaymentMethod.Trim();
        if (!method.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("Online payment isn't available yet. Please pay by cash.");

        var pdfBytes = DecodePdf(request.PdfBase64);

        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            RecipientUserId = request.RecipientUserId,
            RecipientName = request.RecipientName,
            RecipientEmail = request.RecipientEmail,
            RecipientRoleName = request.RecipientRoleName,
            InvoiceType = request.InvoiceType,
            Amount = request.Amount,
            RegularHours = request.RegularHours,
            OvertimeHours = request.OvertimeHours,
            TotalHours = request.TotalHours,
            PeriodFrom = request.PeriodFrom,
            PeriodTo = request.PeriodTo,
            Details = request.Details,
            PdfBase64 = request.PdfBase64,
            Status = "Paid",
            PaymentMethod = "Cash",
            CreatedByUserId = currentUserId,
        };

        invoice.InvoiceId = await _invoices.CreateAsync(invoice, ct);
        invoice.PaidOn = DateTime.UtcNow;
        invoice.CreatedOn = DateTime.UtcNow;

        // Best-effort email: the payment is already recorded and the PDF is
        // downloadable from Accounting even if the email fails.
        try
        {
            var subject = $"Payment received – Invoice {invoice.InvoiceNumber}";
            var html = BuildEmailHtml(invoice);
            var fileName = $"invoice-{Sanitize(invoice.InvoiceNumber)}.pdf";
            await _email.SendInvoiceAsync(invoice.RecipientEmail, subject, html, pdfBytes, fileName, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice {Number} recorded but the email failed.", invoice.InvoiceNumber);
        }

        return Map(invoice);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _invoices.GetAllAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<string?> GetPdfAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _invoices.GetByIdAsync(invoiceId, ct);
        return invoice?.PdfBase64;
    }

    // ---- helpers ----

    private static byte[] DecodePdf(string dataUri)
    {
        var b64 = dataUri?.Trim() ?? string.Empty;
        var comma = b64.IndexOf(',');
        if (b64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            b64 = b64[(comma + 1)..];
        try
        {
            return Convert.FromBase64String(b64);
        }
        catch
        {
            throw AppException.BadRequest("The invoice PDF could not be read.");
        }
    }

    private static string Sanitize(string s) =>
        new string(s.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray());

    private static string BuildEmailHtml(Invoice i)
    {
        var money = i.Amount.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        var isShift = i.InvoiceType.Equals("ShiftPay", StringComparison.OrdinalIgnoreCase);
        var typeLabel = isShift ? "Shift Pay" : "Salary";

        var shiftBlock = string.Empty;
        if (isShift)
        {
            var period = (i.PeriodFrom.HasValue && i.PeriodTo.HasValue)
                ? $"{i.PeriodFrom:MMM d, yyyy} – {i.PeriodTo:MMM d, yyyy}"
                : "—";
            shiftBlock = $"""
                <tr><td style="padding:6px 0;color:#64748b">Pay period</td><td style="text-align:right;font-weight:600">{WebUtility.HtmlEncode(period)}</td></tr>
                <tr><td style="padding:6px 0;color:#64748b">Regular hours</td><td style="text-align:right">{i.RegularHours ?? 0:0.##}</td></tr>
                <tr><td style="padding:6px 0;color:#64748b">Overtime hours</td><td style="text-align:right">{i.OvertimeHours ?? 0:0.##}</td></tr>
                <tr><td style="padding:6px 0;color:#64748b">Total hours</td><td style="text-align:right">{i.TotalHours ?? 0:0.##}</td></tr>
                """;
        }

        return $"""
            <div style="font-family:Inter,Arial,sans-serif;max-width:560px;margin:auto;color:#0f172a">
              <div style="background:#1e1b4b;color:#fff;padding:20px 24px;border-radius:12px 12px 0 0">
                <div style="font-size:18px;font-weight:800">WorkProvider360</div>
                <div style="opacity:.8;font-size:13px">Payment confirmation</div>
              </div>
              <div style="border:1px solid #e2e8f0;border-top:0;border-radius:0 0 12px 12px;padding:24px">
                <p style="margin:0 0 12px">Hi {WebUtility.HtmlEncode(i.RecipientName)},</p>
                <p style="margin:0 0 16px;color:#475569">
                  You have been paid <strong>{money}</strong> ({typeLabel}). A detailed invoice is attached as a PDF.
                </p>
                <table style="width:100%;border-collapse:collapse;font-size:14px;border-top:1px solid #e2e8f0;padding-top:8px">
                  <tr><td style="padding:6px 0;color:#64748b">Invoice #</td><td style="text-align:right;font-weight:600">{WebUtility.HtmlEncode(i.InvoiceNumber)}</td></tr>
                  <tr><td style="padding:6px 0;color:#64748b">Date</td><td style="text-align:right">{i.PaidOn:MMM d, yyyy}</td></tr>
                  <tr><td style="padding:6px 0;color:#64748b">Recipient</td><td style="text-align:right">{WebUtility.HtmlEncode(i.RecipientEmail)}</td></tr>
                  <tr><td style="padding:6px 0;color:#64748b">Payment method</td><td style="text-align:right;font-weight:600">Cash</td></tr>
                  {shiftBlock}
                  <tr><td style="padding:12px 0 0;font-size:16px;font-weight:800">Total paid</td><td style="padding:12px 0 0;text-align:right;font-size:16px;font-weight:800;color:#059669">{money}</td></tr>
                </table>
                <p style="margin:20px 0 0;color:#94a3b8;font-size:12px">
                  This is an automated confirmation from WorkProvider360. The attached PDF is your official receipt.
                </p>
              </div>
            </div>
            """;
    }

    private static InvoiceDto Map(Invoice i) => new()
    {
        InvoiceId = i.InvoiceId,
        InvoiceNumber = i.InvoiceNumber,
        RecipientUserId = i.RecipientUserId,
        RecipientName = i.RecipientName,
        RecipientEmail = i.RecipientEmail,
        RecipientRoleName = i.RecipientRoleName,
        InvoiceType = i.InvoiceType,
        Amount = i.Amount,
        RegularHours = i.RegularHours,
        OvertimeHours = i.OvertimeHours,
        TotalHours = i.TotalHours,
        PeriodFrom = i.PeriodFrom,
        PeriodTo = i.PeriodTo,
        Details = i.Details,
        Status = i.Status,
        PaymentMethod = i.PaymentMethod,
        CreatedOn = i.CreatedOn,
        PaidOn = i.PaidOn,
    };
}
