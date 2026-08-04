/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-04
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.BLL.Common;

/// <summary>
/// Resolves the front-end origin (scheme + host) used to build links inside emails
/// (password reset, login, unlock). Each tenant has its own domain, so we prefer
/// the resolved agency's <c>DomainUrl</c> and fall back to the configured
/// <c>Smtp:ResetPasswordBaseUrl</c> only when the agency has no domain.
/// </summary>
public static class FrontendUrls
{
    /// <summary>
    /// Returns a clean origin like <c>https://agency.example.com</c> (no trailing
    /// slash, no path). <paramref name="agencyDomain"/> wins when present; otherwise
    /// the configured fallback is stripped of any known path suffix.
    /// </summary>
    public static string ResolveOrigin(string? agencyDomain, string? configuredFallback)
    {
        // 1) Per-tenant domain from the master Agencies table.
        if (!string.IsNullOrWhiteSpace(agencyDomain))
            return Normalize(agencyDomain);

        // 2) Fallback: the appsettings value historically includes "/reset-password".
        var configured = (configuredFallback ?? string.Empty).Trim();
        foreach (var suffix in new[] { "/reset-password", "/login" })
        {
            if (configured.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                configured = configured[..^suffix.Length];
                break;
            }
        }

        return string.IsNullOrEmpty(configured) ? string.Empty : configured.TrimEnd('/');
    }

    /// <summary>Ensures a scheme (defaults to https) and removes any trailing slash.</summary>
    private static string Normalize(string domain)
    {
        var d = domain.Trim().TrimEnd('/');
        if (!d.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !d.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            d = "https://" + d;
        }
        return d;
    }
}
