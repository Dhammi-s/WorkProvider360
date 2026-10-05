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
/// Clients (agency side). Access is None/Read/Write from ClientSettings; Admin
/// and Manager are scoped to their own office. Enabling the portal provisions a
/// Users row with the Client role and emails credentials.
/// </summary>
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager}")]
public sealed class ClientsController : BaseApiController
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients) => _clients = clients;

    /// <summary>The caller effective client / service-type permissions.</summary>
    [HttpGet("access")]
    public async Task<ActionResult<ApiResponse<ClientAccessDto>>> GetAccess(CancellationToken ct)
    {
        var access = await _clients.GetAccessAsync(CurrentRoleId, ct);
        return Ok(ApiResponse<ClientAccessDto>.Ok(access));
    }

    /// <summary>Client / visit / portal settings. Readable by any staff role (the scheduler needs the clock flags).</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.User}")]
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<ClientSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _clients.GetSettingsAsync(ct);
        return Ok(ApiResponse<ClientSettingsDto>.Ok(settings));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<ClientSettingsDto>>> UpdateSettings(
        [FromBody] UpsertClientSettingsDto request, CancellationToken ct)
    {
        var settings = await _clients.UpsertSettingsAsync(request, CurrentRoleId, ct);
        return Ok(ApiResponse<ClientSettingsDto>.Ok(settings, "Client settings saved."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ClientDto>>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? officeId = null, [FromQuery] string? status = null,
        [FromQuery] int? serviceTypeId = null, [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _clients.GetPagedAsync(page, pageSize, officeId, status, serviceTypeId, search, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<PagedResultDto<ClientDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var client = await _clients.GetByIdAsync(id, CurrentUserId, CurrentRoleId, ct);
        return client is null
            ? NotFound(ApiResponse.Fail("Client not found."))
            : Ok(ApiResponse<ClientDetailDto>.Ok(client));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Create(
        [FromBody] CreateClientRequestDto request, CancellationToken ct)
    {
        var created = await _clients.CreateAsync(request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ClientDto>.Ok(created, "Client created."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> Update(
        int id, [FromBody] UpdateClientRequestDto request, CancellationToken ct)
    {
        var updated = await _clients.UpdateAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<ClientDto>.Ok(updated, "Client saved."));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateStatus(
        int id, [FromBody] UpdateClientStatusRequestDto request, CancellationToken ct)
    {
        await _clients.UpdateStatusAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("Client status updated."));
    }

    [HttpPost("{id:int}/portal-access")]
    public async Task<ActionResult<ApiResponse<ClientDto>>> SetPortalAccess(
        int id, [FromBody] SetClientPortalAccessRequestDto request, CancellationToken ct)
    {
        var client = await _clients.SetPortalAccessAsync(id, request, CurrentUserId, CurrentRoleId, ct);
        var message = request.Enabled ? "Portal access enabled." : "Portal access disabled.";
        return Ok(ApiResponse<ClientDto>.Ok(client, message));
    }

    [HttpPost("{id:int}/resend-credentials")]
    public async Task<ActionResult<ApiResponse<object?>>> ResendCredentials(int id, CancellationToken ct)
    {
        await _clients.ResendCredentialsAsync(id, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse.Ok("New credentials have been emailed to the client."));
    }

    [HttpGet("{id:int}/eligible-caregivers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EligibleCaregiverDto>>>> GetEligibleCaregivers(
        int id, [FromQuery] int? serviceTypeId, CancellationToken ct)
    {
        var caregivers = await _clients.GetEligibleCaregiversAsync(id, serviceTypeId, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<EligibleCaregiverDto>>.Ok(caregivers));
    }

    [HttpGet("{id:int}/schedules")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientVisitDto>>>> GetSchedules(
        int id, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken ct)
    {
        var visits = await _clients.GetVisitsAsync(id, fromUtc, toUtc, CurrentUserId, CurrentRoleId, ct);
        return Ok(ApiResponse<IReadOnlyList<ClientVisitDto>>.Ok(visits));
    }
}
