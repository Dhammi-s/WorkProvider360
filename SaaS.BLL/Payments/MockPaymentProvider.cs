using System.Security.Cryptography;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Payments;

/// <summary>
/// Sandbox payment provider — no real money. Test-card rules:
///   • card ending 0002 → declined
///   • fewer than 12 digits → invalid
///   • otherwise approved (e.g. 4242 4242 4242 4242)
/// </summary>
public sealed class MockPaymentProvider : IPaymentProvider
{
    public string Name => "Mock";

    public Task<PaymentResult> AuthorizeAndCaptureAsync(PaymentChargeRequest request, CancellationToken ct = default)
    {
        var digits = new string((request.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        var last4 = digits.Length >= 4 ? digits[^4..] : digits;

        if (digits.Length < 12)
            return Task.FromResult(new PaymentResult
            {
                Success = false, Provider = Name, CardLast4 = last4, DeclineReason = "Invalid card number.",
            });

        if (last4 == "0002")
            return Task.FromResult(new PaymentResult
            {
                Success = false, Provider = Name, CardLast4 = last4, DeclineReason = "Card declined (test).",
            });

        var reference = "mock_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
        return Task.FromResult(new PaymentResult
        {
            Success = true, Provider = Name, CardLast4 = last4, ProviderRef = reference,
        });
    }
}
