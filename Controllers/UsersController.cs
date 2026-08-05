/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
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

    /// <summary>Any authenticated user can set their own profile photo (uploaded to Cloudinary).</summary>
    [HttpPost("me/avatar")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateAvatar(
        [FromBody] UpdateAvatarRequestDto request, CancellationToken ct)
    {
        var updated = await _users.UpdateAvatarAsync(CurrentUserId, request.ImageBase64, ct);
        return Ok(ApiResponse<UserDto>.Ok(updated, "Profile photo updated."));
    }

    /// <summary>Only Admins / SuperAdmins can list all users in the tenant.</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetAll(CancellationToken ct)
    {
        var users = await _users.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    /// <summary>Server-side paged list of users.</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<UserDto>>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null, [FromQuery] Guid? officeId = null, [FromQuery] bool noOffice = false,
        CancellationToken ct = default)
    {
        var result = await _users.GetPagedAsync(page, pageSize, role, officeId, noOffice, ct);
        return Ok(ApiResponse<PagedResultDto<UserDto>>.Ok(result));
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

    /// <summary>Resend credentials to many users at once (each gets a new temp password).</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost("resend-credentials")]
    public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> ResendCredentialsBulk(
        [FromBody] BulkResendRequestDto request, CancellationToken ct)
    {
        var result = await _users.ResendCredentialsBulkAsync(request.UserIds, ct);
        var message = $"Sent to {result.Succeeded} of {result.Total} user(s)"
            + (result.Failed > 0 ? $"; {result.Failed} failed." : ".");
        return Ok(ApiResponse<BulkOperationResultDto>.Ok(result, message));
    }

    /// <summary>
    /// Whether Admins/Managers may unlock accounts in this tenant. Lets the Team
    /// page decide whether to show the "Unlock" action to non-SuperAdmins.
    /// </summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    [HttpGet("security-policy")]
    public async Task<ActionResult<ApiResponse<SecurityPolicyDto>>> GetSecurityPolicy(CancellationToken ct)
    {
        var allow = await _users.GetAllowStaffUnlockAsync(ct);
        return Ok(ApiResponse<SecurityPolicyDto>.Ok(new SecurityPolicyDto { AllowStaffUnlock = allow }));
    }

    /// <summary>
    /// Unlocks a locked account. SuperAdmin always; Admin / Manager only when the
    /// tenant flag allows it and the target is below their own role.
    /// </summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
    [HttpPost("{id:int}/unlock")]
    public async Task<ActionResult<ApiResponse<object?>>> Unlock(int id, CancellationToken ct)
    {
        await _users.UnlockAsync(CurrentUserId, CurrentRoleId, id, ct);
        return Ok(ApiResponse.Ok("Account unlocked — a new temporary password has been emailed to the user."));
    }
}
