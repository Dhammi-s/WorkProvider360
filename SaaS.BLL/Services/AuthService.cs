using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordResetTokenRepository _resetTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IEmailService _email;
    private readonly ITenantContext _tenant;
    private readonly JwtSettings _jwtSettings;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordResetTokenRepository resetTokens,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IEmailService email,
        ITenantContext tenant,
        IOptions<JwtSettings> jwtSettings,
        IOptions<SmtpSettings> smtpSettings,
        ILogger<AuthService> logger)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _resetTokens = resetTokens;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _email = email;
        _tenant = tenant;
        _jwtSettings = jwtSettings.Value;
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);

        // Same generic error whether the user is missing, inactive or the
        // password is wrong — avoids leaking which emails are registered.
        if (user is null || !user.IsActive ||
            !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw AppException.Unauthorized("Invalid email or password.");
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
    {
        var principal = _jwt.GetPrincipalFromExpiredToken(request.AccessToken)
            ?? throw AppException.Unauthorized("Invalid access token.");

        var userIdClaim = principal.FindFirst(AppClaimTypes.UserId)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            throw AppException.Unauthorized("Invalid access token.");

        var stored = await _refreshTokens.GetActiveAsync(userId, request.RefreshToken, ct)
            ?? throw AppException.Unauthorized("Refresh token is invalid or expired.");

        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            throw AppException.Unauthorized("User is no longer active.");

        // Rotate: revoke the used refresh token before issuing a new pair.
        await _refreshTokens.RevokeAsync(stored.RefreshTokenId, ct);
        return await IssueTokensAsync(user, ct);
    }

    public Task LogoutAsync(int userId, CancellationToken ct = default)
        => _refreshTokens.RevokeAllForUserAsync(userId, ct);

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);

        // Respond identically whether or not the account exists.
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Forgot-password requested for unknown/inactive email {Email}", request.Email);
            return;
        }

        var rawToken = _jwt.CreateRefreshToken(); // reuse the CSPRNG-based opaque token
        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = rawToken,
            ExpiresOn = DateTime.UtcNow.AddHours(1),
        };
        await _resetTokens.CreateAsync(resetToken, ct);

        var link = BuildResetLink(user.Email, rawToken);

        try
        {
            await _email.SendPasswordResetAsync(user.Email, user.FullName, link, ct);
        }
        catch (Exception ex)
        {
            // Never leak email delivery failures back to the caller.
            _logger.LogError(ex, "Failed sending reset email to {Email}", user.Email);
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct)
            ?? throw AppException.BadRequest("Invalid or expired reset token.");

        var token = await _resetTokens.GetActiveAsync(user.UserId, request.Token, ct)
            ?? throw AppException.BadRequest("Invalid or expired reset token.");

        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        await _users.UpdatePasswordAsync(user.UserId, hash, salt, ct);
        await _resetTokens.MarkUsedAsync(token.PasswordResetTokenId, ct);

        // Force re-authentication everywhere after a password reset.
        await _refreshTokens.RevokeAllForUserAsync(user.UserId, ct);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw AppException.NotFound("User not found.");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            throw AppException.BadRequest("Current password is incorrect.");

        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        await _users.UpdatePasswordAsync(userId, hash, salt, ct);
        await _refreshTokens.RevokeAllForUserAsync(userId, ct);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(AppUser user, CancellationToken ct)
    {
        var agencyId = _tenant.AgencyId;
        var (accessToken, expiresOn) = _jwt.CreateAccessToken(agencyId, user);
        var refreshToken = _jwt.CreateRefreshToken();

        await _refreshTokens.CreateAsync(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            ExpiresOn = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
        }, ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresOn = expiresOn,
            AgencyId = agencyId,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            RoleId = user.RoleId,
            RoleName = user.RoleName ?? string.Empty,
        };
    }

    private string BuildResetLink(string email, string token)
    {
        var baseUrl = _smtpSettings.ResetPasswordBaseUrl.TrimEnd('/');
        var query = $"email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(token)}";
        return $"{baseUrl}?{query}";
    }
}
