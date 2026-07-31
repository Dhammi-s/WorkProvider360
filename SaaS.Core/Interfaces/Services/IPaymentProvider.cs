namespace SaaS.Core.Interfaces.Services;

/// <summary>A card charge to process. Card data is only used to decide approval in the sandbox.</summary>
public sealed class PaymentChargeRequest
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class PaymentResult
{
    public bool Success { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ProviderRef { get; set; }
    public string? CardLast4 { get; set; }
    public string? DeclineReason { get; set; }
}

/// <summary>
/// Abstraction over a payment processor. The sandbox uses a mock; a real bank or
/// Stripe implementation can be dropped in later without touching the POS flow.
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }
    Task<PaymentResult> AuthorizeAndCaptureAsync(PaymentChargeRequest request, CancellationToken ct = default);
}
