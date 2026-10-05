/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>Service-type (skills/services) management. Access from ClientSettings.*ServiceTypeAccess.</summary>
public interface IServiceTypeService
{
    Task<IReadOnlyList<ServiceTypeDto>> GetAllAsync(int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceTypeDto>> GetActiveAsync(CancellationToken ct = default);
    Task<ServiceTypeDto?> GetByIdAsync(int serviceTypeId, CancellationToken ct = default);
    Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ServiceTypeDto> UpdateAsync(int serviceTypeId, UpdateServiceTypeRequestDto request, int roleId, CancellationToken ct = default);
    Task DeactivateAsync(int serviceTypeId, int roleId, CancellationToken ct = default);
}
