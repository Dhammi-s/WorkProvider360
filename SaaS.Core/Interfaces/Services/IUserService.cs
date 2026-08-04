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

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? roleName, Guid? officeId, bool noOffice, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<UserDto> UpdateAvatarAsync(int userId, string imageBase64, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequestDto request, CancellationToken ct = default);

    /// <summary>Public self-registration: creates an active account with the "User" role.</summary>
    Task<UserDto> RegisterUserAsync(RegisterUserRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Resets the user's password to a new temporary one and emails the credentials.
    /// Used by admins to re-send login details (the original password is not recoverable).
    /// </summary>
    Task ResendCredentialsAsync(int userId, CancellationToken ct = default);

    /// <summary>Resend credentials to many users; returns per-batch success/failure counts.</summary>
    Task<BulkOperationResultDto> ResendCredentialsBulkAsync(IReadOnlyList<int> userIds, CancellationToken ct = default);

    /// <summary>Whether Admins/Managers (not only SuperAdmin) may unlock accounts in this tenant.</summary>
    Task<bool> GetAllowStaffUnlockAsync(CancellationToken ct = default);

    /// <summary>Unlocks a locked account, enforcing role/flag rules. SuperAdmin always allowed.</summary>
    Task UnlockAsync(int actingRoleId, int targetUserId, CancellationToken ct = default);

    /// <summary>
    /// Creates the first SuperAdmin for the current tenant. Fails if the tenant
    /// already has any user (so the endpoint self-disables after first use).
    /// </summary>
    Task<UserDto> BootstrapFirstAdminAsync(BootstrapAdminRequestDto request, CancellationToken ct = default);
}
