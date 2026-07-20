using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Infrastructure;

/// <summary>
/// Resolves a tenant (agency) and populates the <see cref="ITenantContext"/>.
/// </summary>
public interface ITenantResolver
{
    /// <summary>Resolve a tenant from the incoming request host / domain url.</summary>
    Task<Agency?> ResolveByDomainAsync(string domainOrHost, CancellationToken ct = default);

    /// <summary>Resolve a tenant from the agency id carried in the JWT.</summary>
    Task<Agency?> ResolveByAgencyIdAsync(int agencyId, CancellationToken ct = default);
}
