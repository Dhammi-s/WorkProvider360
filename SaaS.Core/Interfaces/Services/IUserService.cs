using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequestDto request, CancellationToken ct = default);

    /// <summary>Public self-registration: creates an active account with the "User" role.</summary>
    Task<UserDto> RegisterUserAsync(RegisterUserRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Creates the first SuperAdmin for the current tenant. Fails if the tenant
    /// already has any user (so the endpoint self-disables after first use).
    /// </summary>
    Task<UserDto> BootstrapFirstAdminAsync(BootstrapAdminRequestDto request, CancellationToken ct = default);
}
