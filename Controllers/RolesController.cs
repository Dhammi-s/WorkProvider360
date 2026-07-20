using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Role catalog for the current tenant. Reads are available to any authenticated
/// user; creating a role is restricted to SuperAdmin / Admin.
/// </summary>
[Authorize]
public sealed class RolesController : BaseApiController
{
    private readonly IRoleService _roles;

    public RolesController(IRoleService roles) => _roles = roles;

    /// <summary>List all roles (e.g. to populate a dropdown).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetAll(CancellationToken ct)
    {
        var roles = await _roles.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(id, ct);
        return role is null
            ? NotFound(ApiResponse.Fail("Role not found."))
            : Ok(ApiResponse<RoleDto>.Ok(role));
    }

    /// <summary>
    /// Create a role. The id is supplied explicitly so it stays consistent
    /// across tenants (roles are treated as static).
    /// </summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(
        [FromBody] CreateRoleRequestDto request, CancellationToken ct)
    {
        var created = await _roles.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.RoleId },
            ApiResponse<RoleDto>.Ok(created, "Role created."));
    }
}
