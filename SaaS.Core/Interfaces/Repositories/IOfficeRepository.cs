using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Office data access against the current TENANT database.</summary>
public interface IOfficeRepository
{
    Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default);
    Task<Office?> GetByIdAsync(Guid officeId, CancellationToken ct = default);
    Task<Guid> CreateAsync(Office office, CancellationToken ct = default);
    Task UpdateAsync(Office office, CancellationToken ct = default);
    Task DeactivateAsync(Guid officeId, CancellationToken ct = default);
    Task<IReadOnlyList<AppUser>> GetMembersAsync(Guid officeId, CancellationToken ct = default);
}
