using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Agency branding (logo) + the login page's marketing content. The logo and
/// the public login payload are readable anonymously; only a SuperAdmin edits them.
/// </summary>
[Authorize]
public sealed class BrandingController : BaseApiController
{
    private readonly IBrandingService _branding;
    private readonly ILoginContentService _loginContent;

    public BrandingController(IBrandingService branding, ILoginContentService loginContent)
    {
        _branding = branding;
        _loginContent = loginContent;
    }

    /// <summary>Public: everything the login page renders (agency name, logo, content).</summary>
    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<ActionResult<ApiResponse<PublicLoginPageDto>>> LoginPage(CancellationToken ct)
    {
        var page = await _loginContent.GetPublicAsync(ct);
        return Ok(ApiResponse<PublicLoginPageDto>.Ok(page));
    }

    /// <summary>SuperAdmin: current login-page content for the editor.</summary>
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpGet("login-content")]
    public async Task<ActionResult<ApiResponse<LoginContentDto>>> GetLoginContent(CancellationToken ct)
    {
        var content = await _loginContent.GetForEditAsync(ct);
        return Ok(ApiResponse<LoginContentDto>.Ok(content));
    }

    /// <summary>SuperAdmin: save the login-page content.</summary>
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("login-content")]
    public async Task<ActionResult<ApiResponse<LoginContentDto>>> UpdateLoginContent(
        [FromBody] UpdateLoginContentDto request, CancellationToken ct)
    {
        var content = await _loginContent.UpdateAsync(request, ct);
        return Ok(ApiResponse<LoginContentDto>.Ok(content, "Login page updated."));
    }

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
