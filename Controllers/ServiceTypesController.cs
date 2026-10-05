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
/// Service types (the tenant list of services / skills). Access is None/Read/Write
/// from ClientSettings, enforced in the service; SuperAdmin always has full write.
/// </summary>
[Route("api/service-types")]
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
public sealed class ServiceTypesController : BaseApiController
{
    private readonly IServiceTypeService _serviceTypes;

    public ServiceTypesController(IServiceTypeService serviceTypes) => _serviceTypes = serviceTypes;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceTypeDto>>>> GetAll(CancellationToken ct)
    {
        var items = await _serviceTypes.GetAllAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<ServiceTypeDto>>.Ok(items));
    }

    /// <summary>Active service types for pickers (scheduler, client + profile forms).</summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceTypeDto>>>> GetActive(CancellationToken ct)
    {
        var items = await _serviceTypes.GetActiveAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ServiceTypeDto>>.Ok(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceTypeDto>>> GetById(int id, CancellationToken ct)
    {
        var item = await _serviceTypes.GetByIdAsync(id, ct);
        return item is null
            ? NotFound(ApiResponse.Fail("Service type not found."))
            : Ok(ApiResponse<ServiceTypeDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ServiceTypeDto>>> Create(
        [FromBody] CreateServiceTypeRequestDto request, CancellationToken ct)
    {
        var created = await _serviceTypes.CreateAsync(request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ServiceTypeDto>.Ok(created, "Service type created."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ServiceTypeDto>>> Update(
        int id, [FromBody] UpdateServiceTypeRequestDto request, CancellationToken ct)
    {
        var updated = await _serviceTypes.UpdateAsync(id, request, CurrentRoleId, ct);
        return Ok(ApiResponse<ServiceTypeDto>.Ok(updated, "Service type saved."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> Deactivate(int id, CancellationToken ct)
    {
        await _serviceTypes.DeactivateAsync(id, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("Service type deactivated."));
    }
}
