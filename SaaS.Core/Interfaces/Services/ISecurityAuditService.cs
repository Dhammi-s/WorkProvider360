/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>Records and reports security events (login attempts, detected attacks).</summary>
public interface ISecurityAuditService
{
    /// <summary>Best-effort log of one security event. Never throws.</summary>
    Task LogAsync(
        string eventType,
        string? email = null,
        int? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? path = null,
        string? detail = null,
        CancellationToken ct = default);

    /// <summary>Aggregated dashboard payload (SuperAdmin).</summary>
    Task<SecurityStatsDto> GetStatsAsync(int recentTake = 200, CancellationToken ct = default);
}
