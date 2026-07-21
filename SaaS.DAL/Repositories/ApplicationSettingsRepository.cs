using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class ApplicationSettingsRepository : IApplicationSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ApplicationSettingsRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<ApplicationSettings?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<ApplicationSettings>(
            new CommandDefinition("usp_ApplicationSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<ApplicationSettings> UpsertAsync(ApplicationSettings settings, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<ApplicationSettings>(
            new CommandDefinition("usp_ApplicationSettings_Upsert",
                new
                {
                    settings.RequirePhone,
                    settings.RequireAddress,
                    settings.EmailNotificationsEnabled,
                    settings.NotificationEmail
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
