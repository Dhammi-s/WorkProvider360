using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<long> CreateAsync(RefreshToken token, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition("usp_RefreshToken_Create",
                new { token.UserId, token.Token, token.ExpiresOn },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<RefreshToken?> GetActiveAsync(int userId, string token, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<RefreshToken>(
            new CommandDefinition("usp_RefreshToken_GetActive",
                new { UserId = userId, Token = token },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task RevokeAsync(long refreshTokenId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_RefreshToken_Revoke", new { RefreshTokenId = refreshTokenId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_RefreshToken_RevokeAllForUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
