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
