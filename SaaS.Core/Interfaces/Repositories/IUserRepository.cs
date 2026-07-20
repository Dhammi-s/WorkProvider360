using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>
/// User data access against the current TENANT database via stored procedures.
/// </summary>
public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser?> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken ct = default);
    Task<int> CreateAsync(AppUser user, CancellationToken ct = default);
    Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
