/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Client + client-service-type data access against the current TENANT database.</summary>
public interface IClientRepository
{
    Task<int> CreateAsync(Client client, CancellationToken ct = default);
    Task UpdateAsync(Client client, CancellationToken ct = default);
    Task UpdateStatusAsync(int clientId, string status, CancellationToken ct = default);
    Task SetPortalUserAsync(int clientId, int? userId, bool portalEnabled, CancellationToken ct = default);
    Task<Client?> GetByIdAsync(int clientId, CancellationToken ct = default);
    Task<Client?> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<(IReadOnlyList<Client> Items, int Total)> GetPagedAsync(int page, int pageSize, Guid? officeId, string? status, int? serviceTypeId, string? search, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeClientId, CancellationToken ct = default);
    Task<IReadOnlyList<EligibleCaregiver>> GetEligibleCaregiversAsync(int clientId, int? serviceTypeId, CancellationToken ct = default);

    // Requested services (join table)
    Task ReplaceServiceTypesAsync(int clientId, string serviceTypeIdsJson, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceType>> GetServiceTypesAsync(int clientId, CancellationToken ct = default);
}
