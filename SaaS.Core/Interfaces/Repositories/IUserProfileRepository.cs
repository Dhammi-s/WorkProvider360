/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Staff profile, skills and availability against the current TENANT database.</summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetAsync(int userId, CancellationToken ct = default);
    Task<UserProfile> UpsertAsync(UserProfile profile, CancellationToken ct = default);

    Task ReplaceServiceTypesAsync(int userId, string serviceTypeIdsJson, CancellationToken ct = default);
    Task<IReadOnlyList<ServiceType>> GetServiceTypesAsync(int userId, CancellationToken ct = default);

    Task ReplaceAvailabilityAsync(int userId, string slotsJson, CancellationToken ct = default);
    Task<IReadOnlyList<AvailabilitySlot>> GetAvailabilityAsync(int userId, CancellationToken ct = default);
}
