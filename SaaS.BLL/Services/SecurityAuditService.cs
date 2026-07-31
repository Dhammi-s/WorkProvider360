using Microsoft.Extensions.Logging;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Records security events to the tenant audit log and builds the SuperAdmin
/// dashboard summary. Logging is best-effort: a failure to write an audit row
/// (e.g. tenant not resolved) must never break the request being audited.
/// </summary>
public sealed class SecurityAuditService : ISecurityAuditService
{
    private readonly ISecurityEventRepository _events;
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ISecurityEventRepository events, ILogger<SecurityAuditService> logger)
    {
        _events = events;
        _logger = logger;
    }

    public async Task LogAsync(
        string eventType,
        string? email = null,
        int? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? path = null,
        string? detail = null,
        CancellationToken ct = default)
    {
        try
        {
            await _events.CreateAsync(new SecurityEvent
            {
                EventType = eventType,
                Email = Trim(email, 256),
                UserId = userId,
                IpAddress = Trim(ipAddress, 64),
                UserAgent = Trim(userAgent, 512),
                Path = Trim(path, 300),
                Detail = Trim(detail, 1000),
            }, ct);
        }
        catch (Exception ex)
        {
            // Never surface audit-logging failures to the caller.
            _logger.LogWarning(ex, "Failed to record security event {EventType}.", eventType);
        }
    }

    public async Task<SecurityStatsDto> GetStatsAsync(int recentTake = 200, CancellationToken ct = default)
    {
        var typeCounts = await _events.GetTypeCountsAsync(ct);
        var loginStats = await _events.GetLoginStatsAsync(ct);
        var recent = await _events.GetRecentAsync(recentTake, ct);

        int CountOf(string type) => typeCounts.FirstOrDefault(t => t.EventType == type)?.Count ?? 0;

        return new SecurityStatsDto
        {
            TotalEvents = typeCounts.Sum(t => t.Count),
            TotalLogins = CountOf(SecurityEventTypes.LoginSuccess),
            FailedLogins = CountOf(SecurityEventTypes.LoginFailed),
            Unauthorized = CountOf(SecurityEventTypes.Unauthorized),
            SqlInjectionAttempts = CountOf(SecurityEventTypes.SqlInjection),
            DosAttempts = CountOf(SecurityEventTypes.DosAttempt),
            TypeCounts = typeCounts,
            LoginStats = loginStats,
            Recent = recent.Select(Map).ToList(),
        };
    }

    private static SecurityEventDto Map(SecurityEvent e) => new()
    {
        SecurityEventId = e.SecurityEventId,
        EventType = e.EventType,
        Email = e.Email,
        UserId = e.UserId,
        IpAddress = e.IpAddress,
        UserAgent = e.UserAgent,
        Path = e.Path,
        Detail = e.Detail,
        CreatedOn = e.CreatedOn,
    };

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }
}
