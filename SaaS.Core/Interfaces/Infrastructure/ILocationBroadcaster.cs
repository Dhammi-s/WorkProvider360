/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
