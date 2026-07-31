using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Security audit dashboard data. SuperAdmin only: login analytics and detected
/// attacks (SQL-injection attempts, DoS spikes, unauthorized access) for the tenant.
/// </summary>
[Authorize(Roles = RoleConstants.SuperAdmin)]
public sealed class SecurityController : BaseApiController
{
    private readonly ISecurityAuditService _security;

    public SecurityController(ISecurityAuditService security) => _security = security;

    /// <summary>Aggregated stats + recent events for the security dashboard.</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<SecurityStatsDto>>> Stats(
        [FromQuery] int take = 200, CancellationToken ct = default)
    {
        var stats = await _security.GetStatsAsync(take, ct);
        return Ok(ApiResponse<SecurityStatsDto>.Ok(stats));
    }
}
