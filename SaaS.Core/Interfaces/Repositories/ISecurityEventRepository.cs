/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Security audit data access against the current TENANT database.</summary>
public interface ISecurityEventRepository
{
    Task CreateAsync(SecurityEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityEvent>> GetRecentAsync(int take, CancellationToken ct = default);
    Task<IReadOnlyList<SecurityTypeCountDto>> GetTypeCountsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SecurityLoginStatDto>> GetLoginStatsAsync(CancellationToken ct = default);
}
