using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Offices for the current tenant. SuperAdmin manages all offices; an Admin may
/// view/edit only their own. Create/delete are SuperAdmin-only. Fine-grained
/// scoping (Admin → own office) is enforced in <see cref="IOfficeService"/>.
/// </summary>
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
public sealed class OfficesController : BaseApiController
{
    private readonly IOfficeService _offices;

    public OfficesController(IOfficeService offices) => _offices = offices;

    /// <summary>Active timezones for the office dropdown (any authenticated staff).</summary>
    [Authorize]
    [HttpGet("timezones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TimezoneDto>>>> Timezones(CancellationToken ct)
    {
        var zones = await _offices.GetTimezonesAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TimezoneDto>>.Ok(zones));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OfficeDto>>>> GetAll(CancellationToken ct)
    {
        var offices = await _offices.GetAllAsync(CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<OfficeDto>>.Ok(offices));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OfficeDto>>> GetById(Guid id, CancellationToken ct)
    {
        var office = await _offices.GetByIdAsync(id, CurrentUserId, CurrentRoleId, ct);
        return office is null
            ? NotFound(ApiResponse.Fail("Office not found."))
            : Ok(ApiResponse<OfficeDto>.Ok(office));
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OfficeMemberDto>>>> GetMembers(Guid id, CancellationToken ct)
    {
        var members = await _offices.GetMembersAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<OfficeMemberDto>>.Ok(members));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OfficeDto>>> Create(
        [FromBody] CreateOfficeRequestDto request, CancellationToken ct)
    {
        var created = await _offices.CreateAsync(request, ct);
        return Ok(ApiResponse<OfficeDto>.Ok(created, "Office created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OfficeDto>>> Update(
        Guid id, [FromBody] UpdateOfficeRequestDto request, CancellationToken ct)
    {
        var updated = await _offices.UpdateAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<OfficeDto>.Ok(updated, "Office saved."));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Deactivate(Guid id, CancellationToken ct)
    {
        await _offices.DeactivateAsync(id, ct);
        return Ok(ApiResponse.Ok("Office deactivated."));
    }
}
