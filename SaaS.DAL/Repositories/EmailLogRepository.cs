using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class EmailLogRepository : IEmailLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EmailLogRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task CreateAsync(EmailLog log, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_EmailLog_Create",
                new
                {
                    log.ToAddress,
                    log.Subject,
                    log.Body,
                    log.Status,
                    log.ErrorMessage
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<EmailLog>> GetRecentAsync(int top, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<EmailLog>(
            new CommandDefinition("usp_EmailLog_GetRecent", new { Top = top },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<(IReadOnlyList<EmailLog> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        using var multi = await db.QueryMultipleAsync(
            new CommandDefinition("usp_EmailLog_GetPaged", new { Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        var items = (await multi.ReadAsync<EmailLog>()).AsList();
        var total = await multi.ReadFirstAsync<int>();
        return (items, total);
    }
}
