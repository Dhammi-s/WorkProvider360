using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Infrastructure;

/// <summary>
/// Resolves the current tenant from the master database and stores it in the
/// scoped <see cref="ITenantContext"/>. Only active, non-archived agencies
/// resolve successfully.
/// </summary>
public sealed class TenantResolver : ITenantResolver
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly ITenantContext _tenantContext;

    public TenantResolver(IAgencyRepository agencyRepository, ITenantContext tenantContext)
    {
        _agencyRepository = agencyRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Agency?> ResolveByDomainAsync(string domainOrHost, CancellationToken ct = default)
    {
        var host = NormalizeHost(domainOrHost);
        var agency = await _agencyRepository.GetByDomainAsync(host, ct);
        return Apply(agency);
    }

    public async Task<Agency?> ResolveByAgencyIdAsync(int agencyId, CancellationToken ct = default)
    {
        var agency = await _agencyRepository.GetByIdAsync(agencyId, ct);
        return Apply(agency);
    }

    private Agency? Apply(Agency? agency)
    {
        if (agency is null || !agency.IsActive || agency.IsArchived)
            return null;

        _tenantContext.SetTenant(agency);
        return agency;
    }

    /// <summary>
    /// Reduces "https://Foo.com:443/path" to "foo.com" so it can match the
    /// DomainUrl column regardless of scheme, port, path or casing.
    /// </summary>
    private static string NormalizeHost(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var host = value.Trim();

        if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
            host = uri.Host;
        else
        {
            var schemeIndex = host.IndexOf("://", StringComparison.Ordinal);
            if (schemeIndex >= 0)
                host = host[(schemeIndex + 3)..];

            var slash = host.IndexOf('/');
            if (slash >= 0)
                host = host[..slash];

            var colon = host.IndexOf(':');
            if (colon >= 0)
                host = host[..colon];
        }

        return host.ToLowerInvariant();
    }
}
