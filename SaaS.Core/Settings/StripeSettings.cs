namespace SaaS.Core.Settings;

/// <summary>Stripe keys (test). Used by the Phase 2 online-payment flow.</summary>
public sealed class StripeSettings
{
    public const string SectionName = "Stripe";

    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
