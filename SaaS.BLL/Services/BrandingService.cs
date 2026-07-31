/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class BrandingService : IBrandingService
{
    /// <summary>Cap the stored data URI (~3MB of text ≈ a ~2MB image). Keep logos small.</summary>
    private const int MaxLogoChars = 3_000_000;

    private readonly IBrandingRepository _branding;
    private readonly ICloudinaryService _cloudinary;

    public BrandingService(IBrandingRepository branding, ICloudinaryService cloudinary)
    {
        _branding = branding;
        _cloudinary = cloudinary;
    }

    public async Task<BrandingDto> GetAsync(CancellationToken ct = default)
    {
        var b = await _branding.GetAsync(ct);
        return new BrandingDto { LogoBase64 = b?.LogoBase64, UpdatedOn = b?.UpdatedOn };
    }

    public async Task<BrandingDto> UpdateLogoAsync(string logoBase64, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(logoBase64))
            throw AppException.BadRequest("No image was provided.");

        if (!logoBase64.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("The logo must be an image data URI.");

        if (logoBase64.Length > MaxLogoChars)
            throw AppException.BadRequest("The image is too large. Please crop or use a smaller image.");

        // When Cloudinary is configured, host the logo there and store its URL;
        // otherwise fall back to storing the base64 data URI in the tenant DB.
        var toStore = _cloudinary.IsConfigured
            ? await _cloudinary.UploadImageAsync(logoBase64, "workprovider360/logos", ct)
            : logoBase64;

        var saved = await _branding.UpsertLogoAsync(toStore, ct);
        return new BrandingDto { LogoBase64 = saved.LogoBase64, UpdatedOn = saved.UpdatedOn };
    }
}
