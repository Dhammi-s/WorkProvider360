using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetTokenRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<long> CreateAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition("usp_PasswordResetToken_Create",
                new { token.UserId, token.Token, token.ExpiresOn },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<PasswordResetToken?> GetActiveAsync(int userId, string token, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<PasswordResetToken>(
            new CommandDefinition("usp_PasswordResetToken_GetActive",
                new { UserId = userId, Token = token },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task MarkUsedAsync(long passwordResetTokenId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_PasswordResetToken_MarkUsed",
                new { PasswordResetTokenId = passwordResetTokenId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
