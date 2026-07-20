using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var roles = await db.QueryAsync<Role>(
            new CommandDefinition("usp_Role_GetAll", commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        return roles.AsList();
    }

    public async Task<Role?> GetByIdAsync(int roleId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Role>(
            new CommandDefinition("usp_Role_GetById", new { RoleId = roleId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var count = await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Role_NameExists", new { RoleName = roleName },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return count > 0;
    }

    public async Task<int> CreateAsync(Role role, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_Role_Create",
                new { role.RoleId, role.RoleName, role.IsActive },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
