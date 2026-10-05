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

/// <summary>
/// Client management (agency side). Access is role-based via ClientSettings:
/// SuperAdmin always full; Admin/Manager get None/Read/Write and are scoped to
/// their own office. Portal access provisions a Users row with the Client role.
/// </summary>
public interface IClientService
{
    Task<ClientAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default);
    Task<ClientSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task<ClientSettingsDto> UpsertSettingsAsync(UpsertClientSettingsDto request, int roleId, CancellationToken ct = default);

    Task<PagedResultDto<ClientDto>> GetPagedAsync(int page, int pageSize, Guid? officeId, string? status, int? serviceTypeId, string? search, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ClientDetailDto?> GetByIdAsync(int clientId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(CreateClientRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task<ClientDto> UpdateAsync(int clientId, UpdateClientRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task UpdateStatusAsync(int clientId, UpdateClientStatusRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);

    Task<ClientDto> SetPortalAccessAsync(int clientId, SetClientPortalAccessRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
    Task ResendCredentialsAsync(int clientId, int currentUserId, int roleId, CancellationToken ct = default);

    Task<IReadOnlyList<EligibleCaregiverDto>> GetEligibleCaregiversAsync(int clientId, int? serviceTypeId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<ClientVisitDto>> GetVisitsAsync(int clientId, DateTime? fromUtc, DateTime? toUtc, int currentUserId, int roleId, CancellationToken ct = default);
}
