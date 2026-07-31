using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;
using WebApplication1.Infrastructure;

namespace WebApplication1.Controllers;

/// <summary>
/// Authentication endpoints. Anonymous endpoints (login, forgot/reset) are
/// tenant-resolved from the request host; authenticated endpoints from the JWT.
/// </summary>
public sealed class AuthController : BaseApiController
{
    private readonly IAuthService _auth;
    private readonly ISecurityAuditService _security;

    public AuthController(IAuthService auth, ISecurityAuditService security)
    {
        _auth = auth;
        _security = security;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto request, CancellationToken ct)
    {
        var ip = HttpContext.GetRealIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();
        try
        {
            var result = await _auth.LoginAsync(request, ct);
            await _security.LogAsync(SecurityEventTypes.LoginSuccess, email: request.Email,
                ipAddress: ip, userAgent: userAgent, path: "/api/auth/login", ct: ct);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
        }
        catch (Exception ex)
        {
            await _security.LogAsync(SecurityEventTypes.LoginFailed, email: request.Email,
                ipAddress: ip, userAgent: userAgent, path: "/api/auth/login", detail: ex.Message, ct: ct);
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(
        [FromBody] RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await _auth.RefreshTokenAsync(request, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed."));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        await _auth.ForgotPasswordAsync(request, ct);
        // Always the same response, regardless of whether the email exists.
        return Ok(ApiResponse.Ok("If the email is registered, a reset link has been sent."));
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword(
        [FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        await _auth.ResetPasswordAsync(request, ct);
        return Ok(ApiResponse.Ok("Password has been reset. Please sign in."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ChangePassword(
        [FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        await _auth.ChangePasswordAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout(CancellationToken ct)
    {
        await _auth.LogoutAsync(CurrentUserId, ct);
        return Ok(ApiResponse.Ok("Logged out."));
    }
}
