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
