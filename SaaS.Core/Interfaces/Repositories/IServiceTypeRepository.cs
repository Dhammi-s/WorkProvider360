/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Service-type (skills/services) data access against the current TENANT database.</summary>
public interface IServiceTypeRepository
{
    Task<IReadOnlyList<ServiceType>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ServiceType>> GetActiveAsync(CancellationToken ct = default);
    Task<ServiceType?> GetByIdAsync(int serviceTypeId, CancellationToken ct = default);
    Task<int> CreateAsync(ServiceType serviceType, int? createdByUserId, CancellationToken ct = default);
    Task UpdateAsync(ServiceType serviceType, CancellationToken ct = default);
    Task DeactivateAsync(int serviceTypeId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeServiceTypeId, CancellationToken ct = default);
}
