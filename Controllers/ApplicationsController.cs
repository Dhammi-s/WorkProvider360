using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Role-application workflow. The form config + submission are anonymous
/// (tenant resolved by host); review actions require SuperAdmin / Admin.
/// </summary>
[Authorize]
public sealed class ApplicationsController : BaseApiController
{
    private readonly IApplicationService _applications;

    public ApplicationsController(IApplicationService applications) => _applications = applications;

    /// <summary>Public: roles, custom questions and mandatory-field flags for the form.</summary>
    [AllowAnonymous]
    [HttpGet("form-config")]
    public async Task<ActionResult<ApiResponse<PublicFormConfigDto>>> FormConfig(CancellationToken ct)
    {
        var config = await _applications.GetPublicFormConfigAsync(ct);
        return Ok(ApiResponse<PublicFormConfigDto>.Ok(config));
    }

    /// <summary>Public: submit an application for Admin/Manager access.</summary>
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Submit(
        [FromBody] SubmitApplicationRequestDto request, CancellationToken ct)
    {
        var id = await _applications.SubmitAsync(request, ct);
        return Ok(ApiResponse<object?>.Ok(new { applicationId = id },
            "Application submitted. We'll be in touch by email."));
    }

    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApplicationListItemDto>>>> GetAll(
        [FromQuery] string? status, CancellationToken ct)
    {
        var apps = await _applications.GetAllAsync(status, ct);
        return Ok(ApiResponse<IReadOnlyList<ApplicationListItemDto>>.Ok(apps));
    }

    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ApplicationDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var app = await _applications.GetByIdAsync(id, ct);
        return app is null
            ? NotFound(ApiResponse.Fail("Application not found."))
            : Ok(ApiResponse<ApplicationDetailDto>.Ok(app));
    }

    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<ApiResponse<object?>>> Approve(
        int id, [FromBody] ApproveApplicationRequestDto? request, CancellationToken ct)
    {
        await _applications.ApproveAsync(id, CurrentUserId, CurrentRoleId, request?.OfficeId, ct);
        return Ok(ApiResponse.Ok("Application approved. Credentials have been emailed to the applicant."));
    }

    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<ApiResponse<object?>>> Reject(
        int id, [FromBody] RejectApplicationRequestDto request, CancellationToken ct)
    {
        await _applications.RejectAsync(id, CurrentUserId, request.Reason, ct);
        return Ok(ApiResponse.Ok("Application rejected. The applicant has been notified."));
    }
}
