using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class PosRepository : IPosRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PosRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateAsync(PosTransaction txn, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<Guid>(
            new CommandDefinition("usp_PosTransaction_Create",
                new
                {
                    txn.PayerName,
                    txn.PayerEmail,
                    txn.Description,
                    txn.GrossAmount,
                    txn.FeePercent,
                    txn.FeeFixed,
                    txn.PlatformFee,
                    txn.NetAmount,
                    txn.CardLast4,
                    txn.Status,
                    txn.DeclineReason,
                    txn.Provider,
                    txn.ProviderRef,
                    txn.CreatedByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<PosTransaction>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<PosTransaction>(
            new CommandDefinition("usp_PosTransaction_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<PosFeeSettings?> GetFeeSettingsAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<PosFeeSettings>(
            new CommandDefinition("usp_PosFeeSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<PosFeeSettings> UpsertFeeSettingsAsync(PosFeeSettings settings, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<PosFeeSettings>(
            new CommandDefinition("usp_PosFeeSettings_Upsert",
                new { settings.FeePercent, settings.FeeFixed },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
