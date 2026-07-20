using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<long> CreateAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveAsync(int userId, string token, CancellationToken ct = default);
    Task RevokeAsync(long refreshTokenId, CancellationToken ct = default);
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}
