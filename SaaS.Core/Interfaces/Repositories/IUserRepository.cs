/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
    Task<(IReadOnlyList<AppUser> Items, int Total)> GetPagedAsync(int page, int pageSize, string? roleName, Guid? officeId, bool noOffice, CancellationToken ct = default);
    Task<int> CreateAsync(AppUser user, CancellationToken ct = default);
    Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt, CancellationToken ct = default);
    Task UpdateAvatarAsync(int userId, string avatarUrl, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
