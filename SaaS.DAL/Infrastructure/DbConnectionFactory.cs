using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Settings;

namespace SaaS.DAL.Infrastructure;

/// <summary>
/// Produces open SQL Server connections for either the master database or the
/// current tenant database (resolved via <see cref="ITenantContext"/>).
/// </summary>
public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _masterConnectionString;
    private readonly ITenantContext _tenantContext;

    public DbConnectionFactory(IOptions<MasterDbSettings> masterOptions, ITenantContext tenantContext)
    {
        _masterConnectionString = masterOptions.Value.ConnectionString;
        _tenantContext = tenantContext;
    }

    public async Task<IDbConnection> CreateMasterConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    }

    public async Task<IDbConnection> CreateTenantConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(_tenantContext.ConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
