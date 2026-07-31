/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface IBrandingService
{
    Task<BrandingDto> GetAsync(CancellationToken ct = default);
    Task<BrandingDto> UpdateLogoAsync(string logoBase64, CancellationToken ct = default);
}
