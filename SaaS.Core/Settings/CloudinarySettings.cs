/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Settings;

/// <summary>
/// Cloudinary image-hosting configuration bound from the "Cloudinary" section.
/// Keep the real ApiKey/ApiSecret OUT of the repo — set them via env vars
/// (Cloudinary__ApiKey / Cloudinary__ApiSecret).
/// </summary>
public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName)
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !string.IsNullOrWhiteSpace(ApiSecret)
        && !ApiKey.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase)
        && !ApiSecret.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}
