using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface IBrandingService
{
    Task<BrandingDto> GetAsync(CancellationToken ct = default);
    Task<BrandingDto> UpdateLogoAsync(string logoBase64, CancellationToken ct = default);
}
