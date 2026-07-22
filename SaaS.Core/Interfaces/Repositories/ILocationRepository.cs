using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Location-ping data access against the current TENANT database.</summary>
public interface ILocationRepository
{
    Task<long> CreateAsync(LocationPing ping, CancellationToken ct = default);
    Task<IReadOnlyList<LocationPing>> GetTrailAsync(int scheduleId, CancellationToken ct = default);
    Task<IReadOnlyList<LiveLocation>> GetLiveLatestAsync(int? userId, CancellationToken ct = default);
}
