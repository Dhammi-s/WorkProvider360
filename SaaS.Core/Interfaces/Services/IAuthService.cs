/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Authentication use-cases. All operations run against the tenant resolved into
/// the current <see cref="Infrastructure.ITenantContext"/>.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(int userId, CancellationToken ct = default);

    /// <summary>Always succeeds silently to avoid leaking which emails exist.</summary>
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken ct = default);
}
