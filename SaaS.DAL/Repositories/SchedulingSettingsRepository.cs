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

public sealed class SchedulingSettingsRepository : ISchedulingSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SchedulingSettingsRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<SchedulingSettings?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<SchedulingSettings>(
            new CommandDefinition("usp_SchedulingSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<SchedulingSettings> UpdateAccessAsync(string adminAccess, string managerAccess, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<SchedulingSettings>(
            new CommandDefinition("usp_SchedulingSettings_UpdateAccess",
                new { AdminAccess = adminAccess, ManagerAccess = managerAccess },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<SchedulingSettings> UpdateDefaultsAsync(decimal defaultPayRatePerHour, decimal defaultOvertimeMultiplier, bool notifyAdminOnCreate, bool notifyManagerOnCreate, bool autoClockEnabled, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<SchedulingSettings>(
            new CommandDefinition("usp_SchedulingSettings_UpdateDefaults",
                new
                {
                    DefaultPayRatePerHour = defaultPayRatePerHour,
                    DefaultOvertimeMultiplier = defaultOvertimeMultiplier,
                    NotifyAdminOnCreate = notifyAdminOnCreate,
                    NotifyManagerOnCreate = notifyManagerOnCreate,
                    AutoClockEnabled = autoClockEnabled
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
