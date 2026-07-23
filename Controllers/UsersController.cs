using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Sample tenant-scoped resource demonstrating role-based authorization.
/// All actions require a valid token; some require specific roles.
/// </summary>
[Authorize]
public sealed class UsersController : BaseApiController
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    /// <summary>
    /// One-time bootstrap of the first SuperAdmin for a tenant. Resolved by the
    /// request domain and self-disables once any user exists.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("bootstrap-admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> BootstrapAdmin(
        [FromBody] BootstrapAdminRequestDto request, CancellationToken ct)
    {
        var created = await _users.BootstrapFirstAdminAsync(request, ct);
        return Ok(ApiResponse<UserDto>.Ok(created, "First administrator created."));
    }

    /// <summary>
    /// Public self-registration. Always creates an active account with the
    /// "User" role. Tenant is resolved by the request host.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(
        [FromBody] RegisterUserRequestDto request, CancellationToken ct)
    {
        var created = await _users.RegisterUserAsync(request, ct);
        return Ok(ApiResponse<UserDto>.Ok(created, "Account created. You can now sign in."));
    }

    /// <summary>Any authenticated user can read the current profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me(CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(CurrentUserId, ct);
        return user is null
            ? NotFound(ApiResponse.Fail("User not found."))
            : Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>Only Admins / SuperAdmins can list all users in the tenant.</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAll(CancellationToken ct)
    {
        var users = await _users.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    /// <summary>Only Admins / SuperAdmins can create users.</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(
        [FromBody] CreateUserRequestDto request, CancellationToken ct)
    {
        // An Admin can only create users inside their own office; ignore any
        // office they try to specify and force their own.
        if (CurrentRoleId == RoleConstants.AdminId)
        {
            var me = await _users.GetByIdAsync(CurrentUserId, ct);
            request.OfficeId = me?.OfficeId;
        }

        var created = await _users.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.UserId },
            ApiResponse<UserDto>.Ok(created, "User created."));
    }

    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct);
        return user is null
            ? NotFound(ApiResponse.Fail("User not found."))
            : Ok(ApiResponse<UserDto>.Ok(user));
    }

    /// <summary>
    /// Re-send login credentials to a user by resetting their password to a new
    /// temporary one and emailing it. SuperAdmin / Admin only.
    /// </summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost("{id:int}/resend-credentials")]
    public async Task<ActionResult<ApiResponse<object?>>> ResendCredentials(int id, CancellationToken ct)
    {
        await _users.ResendCredentialsAsync(id, ct);
        return Ok(ApiResponse.Ok("New credentials have been emailed to the user."));
    }
}
