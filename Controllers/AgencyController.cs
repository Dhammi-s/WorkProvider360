using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Repositories;

namespace WebApplication1.Controllers;

/// <summary>Read-only info about the caller's own tenant (agency), for display.</summary>
[Authorize]
public sealed class AgencyController : BaseApiController
{
    private readonly IAgencyRepository _agencies;

    public AgencyController(IAgencyRepository agencies) => _agencies = agencies;

    /// <summary>The current agency's public details (name, location).</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AgencyInfoDto>>> Me(CancellationToken ct)
    {
        var agency = await _agencies.GetByIdAsync(CurrentAgencyId, ct);
        if (agency is null)
            return NotFound(ApiResponse.Fail("Agency not found."));

        return Ok(ApiResponse<AgencyInfoDto>.Ok(new AgencyInfoDto
        {
            AgencyId = agency.AgencyId,
            AgencyName = agency.AgencyName,
            Location = agency.Location,
        }));
    }
}
