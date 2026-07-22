using Microsoft.AspNetCore.SignalR;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Infrastructure;
using WebApplication1.Hubs;

namespace WebApplication1.Infrastructure;

/// <summary>
/// SignalR implementation of <see cref="ILocationBroadcaster"/>. Sends the
/// "locationUpdated" event to the target agency's watcher group.
/// </summary>
public sealed class LocationBroadcaster : ILocationBroadcaster
{
    private readonly IHubContext<LocationHub> _hub;

    public LocationBroadcaster(IHubContext<LocationHub> hub) => _hub = hub;

    public Task BroadcastLiveLocationAsync(int agencyId, LiveLocationDto location, CancellationToken ct = default)
        => _hub.Clients.Group(LocationHub.GroupFor(agencyId)).SendAsync("locationUpdated", location, ct);
}
