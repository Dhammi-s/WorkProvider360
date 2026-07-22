using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LocationRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<long> CreateAsync(LocationPing ping, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition("usp_LocationPing_Create",
                new { ping.ScheduleId, ping.UserId, ping.Latitude, ping.Longitude, ping.AccuracyMeters },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<LocationPing>> GetTrailAsync(int scheduleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<LocationPing>(
            new CommandDefinition("usp_LocationPing_GetTrail", new { ScheduleId = scheduleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<LiveLocation>> GetLiveLatestAsync(int? userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<LiveLocation>(
            new CommandDefinition("usp_LocationPing_GetLiveLatest", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
