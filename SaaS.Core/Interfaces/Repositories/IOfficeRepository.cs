/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
