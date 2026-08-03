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

public sealed class BrandingRepository : IBrandingRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BrandingRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Branding?> GetAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Branding>(
            new CommandDefinition("usp_Branding_Get",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<Branding> UpsertLogoAsync(string logoBase64, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<Branding>(
            new CommandDefinition("usp_Branding_UpsertLogo", new { LogoBase64 = logoBase64 },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
