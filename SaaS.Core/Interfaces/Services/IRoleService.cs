using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoleDto?> GetByIdAsync(int roleId, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(CreateRoleRequestDto request, CancellationToken ct = default);
}
