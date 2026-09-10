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

/// <summary>Service-type data access against the current TENANT database (Dapper + procs).</summary>
public sealed class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ServiceTypeRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<ServiceType>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ServiceType>(
            new CommandDefinition("usp_ServiceType_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<ServiceType>> GetActiveAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ServiceType>(
            new CommandDefinition("usp_ServiceType_GetActive",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<ServiceType?> GetByIdAsync(int serviceTypeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<ServiceType>(
            new CommandDefinition("usp_ServiceType_GetById", new { ServiceTypeId = serviceTypeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> CreateAsync(ServiceType serviceType, int? createdByUserId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_ServiceType_Create",
                new
                {
                    serviceType.Name,
                    serviceType.Description,
                    serviceType.Category,
                    serviceType.ColorTag,
                    serviceType.SortOrder,
                    CreatedByUserId = createdByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(ServiceType serviceType, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_ServiceType_Update",
                new
                {
                    serviceType.ServiceTypeId,
                    serviceType.Name,
                    serviceType.Description,
                    serviceType.Category,
                    serviceType.ColorTag,
                    serviceType.SortOrder,
                    serviceType.IsActive
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task DeactivateAsync(int serviceTypeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_ServiceType_Deactivate", new { ServiceTypeId = serviceTypeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeServiceTypeId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var count = await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_ServiceType_NameExists",
                new { Name = name, ExcludeServiceTypeId = excludeServiceTypeId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return count > 0;
    }
}
