/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Location-ping data access against the current TENANT database.</summary>
public interface ILocationRepository
{
    Task<long> CreateAsync(LocationPing ping, CancellationToken ct = default);
    Task<IReadOnlyList<LocationPing>> GetTrailAsync(int scheduleId, CancellationToken ct = default);
    Task<IReadOnlyList<LiveLocation>> GetLiveLatestAsync(int? userId, CancellationToken ct = default);
}
