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

    public BrandingService(IBrandingRepository branding) => _branding = branding;

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

        var saved = await _branding.UpsertLogoAsync(logoBase64, ct);
        return new BrandingDto { LogoBase64 = saved.LogoBase64, UpdatedOn = saved.UpdatedOn };
    }
}
