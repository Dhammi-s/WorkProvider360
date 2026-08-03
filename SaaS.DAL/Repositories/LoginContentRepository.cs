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

public sealed class LoginContentRepository : ILoginContentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LoginContentRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<LoginPageContent?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<LoginPageContent>(
            new CommandDefinition("usp_LoginContent_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<LoginPageContent> UpsertAsync(LoginPageContent c, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<LoginPageContent>(
            new CommandDefinition("usp_LoginContent_Upsert",
                new
                {
                    c.HeadlineLead, c.HeadlineHighlight, c.HeadlineTrail, c.Subtitle,
                    c.Stat1Label, c.Stat1Value, c.Stat2Label, c.Stat2Value, c.Stat3Label, c.Stat3Value,
                    c.QuoteText, c.QuoteAuthor, c.QuoteRole
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
