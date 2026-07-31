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

/// <summary>
/// Office management. SuperAdmin manages all offices; an Admin may view/edit only
/// their own office. All methods receive the caller's identity for enforcement.
/// </summary>
public interface IOfficeService
{
    Task<IReadOnlyList<TimezoneDto>> GetTimezonesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<OfficeDto>> GetAllAsync(int currentUserId, int roleId, CancellationToken ct = default);
    Task<OfficeDto?> GetByIdAsync(Guid officeId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<OfficeMemberDto>> GetMembersAsync(Guid officeId, int currentUserId, int roleId, CancellationToken ct = default);

    Task<OfficeDto> CreateAsync(CreateOfficeRequestDto request, CancellationToken ct = default);
    Task<OfficeDto> UpdateAsync(Guid officeId, UpdateOfficeRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task DeactivateAsync(Guid officeId, CancellationToken ct = default);
}
