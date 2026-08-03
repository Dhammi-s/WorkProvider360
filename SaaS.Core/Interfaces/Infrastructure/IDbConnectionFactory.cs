/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Data;

namespace SaaS.Core.Interfaces.Infrastructure;

/// <summary>
/// Creates open ADO.NET connections. The master connection targets the shared
/// Agencies database; the tenant connection targets the current agency's
/// database resolved into <see cref="ITenantContext"/>.
/// </summary>
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateMasterConnectionAsync(CancellationToken ct = default);

    Task<IDbConnection> CreateTenantConnectionAsync(CancellationToken ct = default);
}
