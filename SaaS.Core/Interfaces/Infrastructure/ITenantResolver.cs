/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
