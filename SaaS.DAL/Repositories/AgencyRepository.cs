using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>
/// Reads agency/tenant metadata from the MASTER database using stored procedures.
/// </summary>
public sealed class AgencyRepository : IAgencyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AgencyRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Agency?> GetByDomainAsync(string domainUrl, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateMasterConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Agency>(
            new CommandDefinition(
                "usp_Agency_GetByDomain",
                new { DomainUrl = domainUrl },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
    }

    public async Task<Agency?> GetByIdAsync(int agencyId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateMasterConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Agency>(
            new CommandDefinition(
                "usp_Agency_GetById",
                new { AgencyId = agencyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
    }
}
