using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Infrastructure;

/// <summary>
/// Scoped, per-request holder for the resolved tenant. Populated either from the
/// request domain (anonymous endpoints) or from the agency id JWT claim.
/// </summary>
public interface ITenantContext
{
    bool IsResolved { get; }
    int AgencyId { get; }
    string ConnectionString { get; }
    Agency? Agency { get; }

    void SetTenant(Agency agency);
}
