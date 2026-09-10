/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// The client portal. Every endpoint resolves the client from the signed-in
/// user id (Client role) and returns only that client own data.
/// </summary>
[Authorize(Roles = RoleConstants.Client)]
public sealed class PortalController : BaseApiController
{
    private readonly IClientPortalService _portal;

    public PortalController(IClientPortalService portal) => _portal = portal;

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PortalProfileDto>>> Me(CancellationToken ct)
    {
        var me = await _portal.GetMeAsync(CurrentUserId, ct);
        return Ok(ApiResponse<PortalProfileDto>.Ok(me));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<PortalProfileDto>>> UpdateMe(
        [FromBody] UpdatePortalProfileRequestDto request, CancellationToken ct)
    {
        var me = await _portal.UpdateMeAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<PortalProfileDto>.Ok(me, "Profile saved."));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<PortalDashboardDto>>> Dashboard(CancellationToken ct)
    {
        var dashboard = await _portal.GetDashboardAsync(CurrentUserId, ct);
        return Ok(ApiResponse<PortalDashboardDto>.Ok(dashboard));
    }

    [HttpGet("visits")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientVisitDto>>>> Visits(
        [FromQuery] string? status, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken ct)
    {
        var visits = await _portal.GetVisitsAsync(CurrentUserId, status, fromUtc, toUtc, ct);
        return Ok(ApiResponse<IReadOnlyList<ClientVisitDto>>.Ok(visits));
    }

    [HttpGet("visits/{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientVisitDetailDto>>> Visit(int id, CancellationToken ct)
    {
        var visit = await _portal.GetVisitAsync(CurrentUserId, id, ct);
        return Ok(ApiResponse<ClientVisitDetailDto>.Ok(visit));
    }

    [HttpGet("service-types")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceTypeDto>>>> ServiceTypes(CancellationToken ct)
    {
        var services = await _portal.GetServiceTypesAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<ServiceTypeDto>>.Ok(services));
    }
}
