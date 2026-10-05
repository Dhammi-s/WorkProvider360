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

/// <summary>Client data access against the current TENANT database (Dapper + procs).</summary>
public sealed class ClientRepository : IClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    private static object ToParams(Client c) => new
    {
        c.OfficeId,
        c.FirstName,
        c.LastName,
        c.Email,
        c.Phone,
        c.AlternatePhone,
        c.DateOfBirth,
        c.Gender,
        c.AddressLine1,
        c.AddressLine2,
        c.City,
        c.State,
        c.PostalCode,
        c.Country,
        c.Latitude,
        c.Longitude,
        c.EmergencyContactName,
        c.EmergencyContactPhone,
        c.EmergencyContactRelation,
        c.PreferredLanguage,
        c.AccessInstructions,
        c.CareNotes,
        c.Allergies,
        c.MobilityNotes,
        c.Status,
        c.StartDate,
        c.Notes
    };

    public async Task<int> CreateAsync(Client client, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var p = new DynamicParameters(ToParams(client));
        p.Add("CreatedByUserId", client.CreatedByUserId);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Client_Create", p,
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(Client client, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var p = new DynamicParameters(ToParams(client));
        p.Add("ClientId", client.ClientId);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Client_Update", p,
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateStatusAsync(int clientId, string status, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Client_UpdateStatus", new { ClientId = clientId, Status = status },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task SetPortalUserAsync(int clientId, int? userId, bool portalEnabled, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_Client_SetPortalUser",
                new { ClientId = clientId, UserId = userId, PortalEnabled = portalEnabled },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<Client?> GetByIdAsync(int clientId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Client>(
            new CommandDefinition("usp_Client_GetById", new { ClientId = clientId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<Client?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Client>(
            new CommandDefinition("usp_Client_GetByUserId", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<(IReadOnlyList<Client> Items, int Total)> GetPagedAsync(
        int page, int pageSize, Guid? officeId, string? status, int? serviceTypeId, string? search, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        using var multi = await db.QueryMultipleAsync(
            new CommandDefinition("usp_Client_GetPaged",
                new { Page = page, PageSize = pageSize, OfficeId = officeId, Status = status, ServiceTypeId = serviceTypeId, Search = search },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        var items = (await multi.ReadAsync<Client>()).AsList();
        var total = await multi.ReadFirstAsync<int>();
        return (items, total);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeClientId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var count = await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Client_EmailExists",
                new { Email = email, ExcludeClientId = excludeClientId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return count > 0;
    }

    public async Task<IReadOnlyList<EligibleCaregiver>> GetEligibleCaregiversAsync(int clientId, int? serviceTypeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<EligibleCaregiver>(
            new CommandDefinition("usp_Client_GetEligibleCaregivers",
                new { ClientId = clientId, ServiceTypeId = serviceTypeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task ReplaceServiceTypesAsync(int clientId, string serviceTypeIdsJson, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_ClientServiceType_Replace",
                new { ClientId = clientId, ServiceTypeIdsJson = serviceTypeIdsJson },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ServiceType>> GetServiceTypesAsync(int clientId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ServiceType>(
            new CommandDefinition("usp_ClientServiceType_GetByClient", new { ClientId = clientId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
