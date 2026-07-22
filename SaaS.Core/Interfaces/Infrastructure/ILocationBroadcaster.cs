using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Infrastructure;

/// <summary>
/// Pushes a live location update to authorised watchers (managers/admins) in
/// real time. Implemented in the web layer over SignalR; abstracted here so the
/// business layer stays free of ASP.NET dependencies.
/// </summary>
public interface ILocationBroadcaster
{
    Task BroadcastLiveLocationAsync(int agencyId, LiveLocationDto location, CancellationToken ct = default);
}
