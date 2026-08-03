/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class LogSettingsRepository : ILogSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LogSettingsRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<LogSettings?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<LogSettings>(
            new CommandDefinition("usp_LogSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<LogSettings> UpsertAsync(LogSettings settings, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<LogSettings>(
            new CommandDefinition("usp_LogSettings_Upsert",
                new { settings.AdminCanViewLogs, settings.ManagerCanViewLogs },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
