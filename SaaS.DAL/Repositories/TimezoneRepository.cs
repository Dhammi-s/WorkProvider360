using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class TimezoneRepository : ITimezoneRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TimezoneRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<Timezone>> GetActiveAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Timezone>(
            new CommandDefinition("usp_Timezone_GetActive", commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        return rows.AsList();
    }
}
