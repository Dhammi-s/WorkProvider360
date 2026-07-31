using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Agency branding (logo). Any authenticated user can read it (to render the
/// logo); only a SuperAdmin can change it.
/// </summary>
[Authorize]
public sealed class BrandingController : BaseApiController
{
    private readonly IBrandingService _branding;

    public BrandingController(IBrandingService branding) => _branding = branding;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<BrandingDto>>> Get(CancellationToken ct)
    {
        var branding = await _branding.GetAsync(ct);
        return Ok(ApiResponse<BrandingDto>.Ok(branding));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("logo")]
    public async Task<ActionResult<ApiResponse<BrandingDto>>> UpdateLogo(
        [FromBody] UpdateLogoRequestDto request, CancellationToken ct)
    {
        var branding = await _branding.UpdateLogoAsync(request.LogoBase64, ct);
        return Ok(ApiResponse<BrandingDto>.Ok(branding, "Logo updated."));
    }
}
