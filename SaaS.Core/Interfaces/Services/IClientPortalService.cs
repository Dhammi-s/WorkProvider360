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

/// <summary>Read/limited-write endpoints for a signed-in client (RoleId = Client).</summary>
public interface IClientPortalService
{
    Task<PortalProfileDto> GetMeAsync(int userId, CancellationToken ct = default);
    Task<PortalProfileDto> UpdateMeAsync(int userId, UpdatePortalProfileRequestDto request, CancellationToken ct = default);
    Task<PortalDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<ClientVisitDto>> GetVisitsAsync(int userId, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<ClientVisitDetailDto> GetVisitAsync(int userId, int scheduleId, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceTypeDto>> GetServiceTypesAsync(int userId, CancellationToken ct = default);
}
