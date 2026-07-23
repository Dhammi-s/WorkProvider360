using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnnouncementRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateAsync(string title, string message, int? createdByUserId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<Guid>(
            new CommandDefinition("usp_Announcement_Create",
                new { Title = title, Message = message, CreatedByUserId = createdByUserId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Announcement>(
            new CommandDefinition("usp_Announcement_GetActive",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<Announcement>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Announcement>(
            new CommandDefinition("usp_Announcement_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task DeactivateAsync(Guid announcementId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Announcement_Deactivate", new { AnnouncementId = announcementId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<AnnouncementSettings?> GetSettingsAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<AnnouncementSettings>(
            new CommandDefinition("usp_AnnouncementSettings_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<AnnouncementSettings> UpsertSettingsAsync(AnnouncementSettings settings, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<AnnouncementSettings>(
            new CommandDefinition("usp_AnnouncementSettings_Upsert",
                new { settings.ShowToAdmin, settings.ShowToManager, settings.ShowToUser },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
