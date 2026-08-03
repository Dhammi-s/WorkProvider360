/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<long> CreateAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveAsync(int userId, string token, CancellationToken ct = default);
    Task RevokeAsync(long refreshTokenId, CancellationToken ct = default);
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}
