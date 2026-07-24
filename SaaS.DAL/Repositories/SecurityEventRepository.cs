using System.Data;
using Dapper;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class SecurityEventRepository : ISecurityEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SecurityEventRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task CreateAsync(SecurityEvent evt, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_SecurityEvent_Create",
                new
                {
                    evt.EventType,
                    evt.Email,
                    evt.UserId,
                    evt.IpAddress,
                    evt.UserAgent,
                    evt.Path,
                    evt.Detail
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<SecurityEvent>> GetRecentAsync(int take, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<SecurityEvent>(
            new CommandDefinition("usp_SecurityEvent_GetRecent", new { Take = take },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<SecurityTypeCountDto>> GetTypeCountsAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<SecurityTypeCountDto>(
            new CommandDefinition("usp_SecurityEvent_GetTypeCounts",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<SecurityLoginStatDto>> GetLoginStatsAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<SecurityLoginStatDto>(
            new CommandDefinition("usp_SecurityEvent_GetLoginStats",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
