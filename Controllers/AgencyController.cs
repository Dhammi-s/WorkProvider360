/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
      [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Message = "API is working successfully",
            Status = true,
            Time = DateTime.UtcNow
        });
    }
}
