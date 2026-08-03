/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Settings;

/// <summary>
/// Twilio configuration bound from the "Twilio" section of appsettings.
/// Keep the real AccountSid/AuthToken OUT of the repo — set them via environment
/// variables (Twilio__AccountSid / Twilio__AuthToken / Twilio__FromNumber).
/// </summary>
public sealed class TwilioSettings
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>The Twilio phone number SMS is sent from, e.g. "+15551234567".</summary>
    public string FromNumber { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AccountSid)
        && !string.IsNullOrWhiteSpace(AuthToken)
        && !string.IsNullOrWhiteSpace(FromNumber)
        && !AccountSid.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}
