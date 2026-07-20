using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<long> CreateAsync(PasswordResetToken token, CancellationToken ct = default);
    Task<PasswordResetToken?> GetActiveAsync(int userId, string token, CancellationToken ct = default);
    Task MarkUsedAsync(long passwordResetTokenId, CancellationToken ct = default);
}
