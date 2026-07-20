using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(int roleId, CancellationToken ct = default);
}
