/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Infrastructure;

namespace SaaS.DAL.Infrastructure;

/// <summary>
/// Scoped, per-request implementation of <see cref="ITenantContext"/>.
/// Registered as Scoped so each HTTP request gets its own instance.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private Agency? _agency;

    public bool IsResolved => _agency is not null;

    public int AgencyId => _agency?.AgencyId
        ?? throw new AppException("Tenant has not been resolved for this request.", 400);

    public string ConnectionString => _agency?.ConnectionString
        ?? throw new AppException("Tenant has not been resolved for this request.", 400);

    public Agency? Agency => _agency;

    public void SetTenant(Agency agency) => _agency = agency;
}
