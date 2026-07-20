using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>
/// User data access against the current TENANT database using stored procedures.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<AppUser>(
            new CommandDefinition("usp_User_GetByEmail", new { Email = email },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<AppUser?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<AppUser>(
            new CommandDefinition("usp_User_GetById", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var users = await db.QueryAsync<AppUser>(
            new CommandDefinition("usp_User_GetAll", commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        return users.AsList();
    }

    public async Task<int> CreateAsync(AppUser user, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_User_Create",
                new
                {
                    user.Email,
                    user.FullName,
                    user.PasswordHash,
                    user.PasswordSalt,
                    user.RoleId,
                    user.IsActive
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_User_UpdatePassword",
                new { UserId = userId, PasswordHash = passwordHash, PasswordSalt = passwordSalt },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var count = await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_User_EmailExists", new { Email = email },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return count > 0;
    }
}
