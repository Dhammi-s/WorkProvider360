/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class ClientSettingsRepository : IClientSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientSettingsRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<ClientSettings?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<ClientSettings>(
            new CommandDefinition("usp_ClientSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<ClientSettings> UpsertAsync(ClientSettings s, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<ClientSettings>(
            new CommandDefinition("usp_ClientSettings_Upsert",
                new
                {
                    s.AdminClientAccess,
                    s.ManagerClientAccess,
                    s.AdminServiceTypeAccess,
                    s.ManagerServiceTypeAccess,
                    s.AutoClockInEnabled,
                    s.AutoClockOutEnabled,
                    s.RequireClientSignatureOnClockIn,
                    s.RequireClientSignatureOnClockOut,
                    s.RequireSameOffice,
                    s.RequireMatchingSkill,
                    s.CaptureClockLocation,
                    s.ClientPortalEnabled,
                    s.SendClientCredentialsEmail,
                    s.NotifyClientOnSchedule,
                    s.RequireClientEmail,
                    s.RequireClientPhone,
                    s.RequireClientDateOfBirth,
                    s.RequireEmergencyContact,
                    s.RequireClientServiceTypes
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
