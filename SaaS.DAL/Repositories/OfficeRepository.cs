using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>Office data access against the current TENANT database (Dapper + procs).</summary>
public sealed class OfficeRepository : IOfficeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OfficeRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Office>(
            new CommandDefinition("usp_Office_GetAll", commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<Office?> GetByIdAsync(Guid officeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Office>(
            new CommandDefinition("usp_Office_GetById", new { OfficeId = officeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<Guid> CreateAsync(Office office, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<Guid>(
            new CommandDefinition("usp_Office_Create",
                new { office.OfficeName, office.Address, office.Phone, office.TimezoneId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(Office office, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Office_Update",
                new
                {
                    office.OfficeId,
                    office.OfficeName,
                    office.Address,
                    office.Phone,
                    office.TimezoneId,
                    office.IsActive
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task DeactivateAsync(Guid officeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Office_Deactivate", new { OfficeId = officeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<AppUser>> GetMembersAsync(Guid officeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<AppUser>(
            new CommandDefinition("usp_Office_GetMembers", new { OfficeId = officeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
