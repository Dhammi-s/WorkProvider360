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

    /// <summary>Records a failed sign-in; locks the account once the threshold is reached (SuperAdmin exempt).</summary>
    Task RegisterFailedLoginAsync(int userId, int threshold, CancellationToken ct = default);

    /// <summary>Clears the failed-attempt counter after a successful sign-in.</summary>
    Task ResetFailedLoginAsync(int userId, CancellationToken ct = default);

    /// <summary>Manually locks or unlocks a user; unlocking also clears the failed-attempt counter.</summary>
    Task SetLockoutAsync(int userId, bool isLockedOut, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
