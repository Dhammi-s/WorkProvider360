using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SaaS.Core.Constants;

namespace WebApplication1.Hubs;

/// <summary>
/// Real-time channel that pushes live worker locations to authorised watchers.
/// Each connection is placed in a per-agency group; only SuperAdmin/Admin/Manager
/// connections join, so regular users never receive other people's locations.
/// Broadcasts are sent by <c>LocationBroadcaster</c> from the REST ping endpoint.
/// </summary>
[Authorize]
public sealed class LocationHub : Hub
{
    public const string GroupPrefix = "agency-";

    public static string GroupFor(int agencyId) => $"{GroupPrefix}{agencyId}";

    public override async Task OnConnectedAsync()
    {
        var agencyId = Context.User?.FindFirst(AppClaimTypes.AgencyId)?.Value;
        var roleId = Context.User?.FindFirst(AppClaimTypes.RoleId)?.Value;

        var canWatch =
            roleId == RoleConstants.SuperAdminId.ToString() ||
            roleId == RoleConstants.AdminId.ToString() ||
            roleId == RoleConstants.ManagerId.ToString();

        if (!string.IsNullOrEmpty(agencyId) && canWatch)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{GroupPrefix}{agencyId}");

        await base.OnConnectedAsync();
    }
}
