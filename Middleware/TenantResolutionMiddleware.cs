using SaaS.Core.Constants;
using SaaS.Core.Interfaces.Infrastructure;

namespace WebApplication1.Middleware;

/// <summary>
/// Resolves the current tenant into the scoped <see cref="ITenantContext"/>.
///
/// - Authenticated requests resolve by the <c>agency_id</c> JWT claim.
/// - Anonymous requests (login, forgot-password) resolve by the request host,
///   matched against the Agencies.DomainUrl column in the master database.
///
/// Must run AFTER UseAuthentication so the JWT claims are available.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver)
    {
        // Resolution failures must not break endpoints that don't need a tenant
        // (OpenAPI doc, auth challenges, health checks). Endpoints that DO need a
        // tenant will surface a clean 400 when they touch the unresolved context.
        try
        {
            var agencyIdClaim = context.User?.FindFirst(AppClaimTypes.AgencyId)?.Value;

            if (int.TryParse(agencyIdClaim, out var agencyId))
            {
                var byId = await resolver.ResolveByAgencyIdAsync(agencyId, context.RequestAborted);
                if (byId is null)
                    _logger.LogWarning("Agency {AgencyId} from token could not be resolved (inactive/archived).", agencyId);
            }
            else
            {
                // Prefer an explicit override header, then the request Host.
                var host = context.Request.Headers["X-Tenant-Domain"].FirstOrDefault()
                           ?? context.Request.Host.Host;

                var byDomain = await resolver.ResolveByDomainAsync(host, context.RequestAborted);
                if (byDomain is null)
                    _logger.LogWarning("No active agency matched host {Host}.", host);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant resolution failed; continuing without a resolved tenant.");
        }

        await _next(context);
    }
}
