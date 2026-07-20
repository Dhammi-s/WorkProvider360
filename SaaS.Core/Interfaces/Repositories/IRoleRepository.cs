using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(int roleId, CancellationToken ct = default);
    Task<bool> RoleNameExistsAsync(string roleName, CancellationToken ct = default);
    Task<int> CreateAsync(Role role, CancellationToken ct = default);
}
