using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;

namespace WebApplication1.Controllers;

/// <summary>
/// Base for all API controllers. Exposes the current agency id and user id that
/// were embedded in the JWT, so derived controllers never re-parse claims.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Agency (tenant) id carried in the access token. 0 when unauthenticated.</summary>
    protected int CurrentAgencyId => GetInt(AppClaimTypes.AgencyId);

    /// <summary>User id carried in the access token. 0 when unauthenticated.</summary>
    protected int CurrentUserId => GetInt(AppClaimTypes.UserId);

    protected int CurrentRoleId => GetInt(AppClaimTypes.RoleId);

    protected string? CurrentEmail => User.FindFirst(AppClaimTypes.Email)?.Value;

    protected string? CurrentRoleName => User.FindFirst(AppClaimTypes.RoleName)?.Value;

    private int GetInt(string claimType)
        => int.TryParse(User.FindFirst(claimType)?.Value, out var value) ? value : 0;
}
