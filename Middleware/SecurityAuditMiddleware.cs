using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SaaS.Core.Constants;
using SaaS.Core.Interfaces.Services;
using WebApplication1.Infrastructure;

namespace WebApplication1.Middleware;

/// <summary>
/// Best-effort security monitoring. For every real request it:
///   1. flags SQL-injection patterns in the URL/query string,
///   2. flags rate-based DoS spikes per real client IP,
///   3. records unauthorized (401/403) responses.
/// Events are written to the tenant audit log via <see cref="ISecurityAuditService"/>.
/// All detection is wrapped so it can never break the request it is auditing.
/// </summary>
public sealed class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;

    // Sliding per-IP request counter for the DoS heuristic (in-memory, process-local).
    private static readonly ConcurrentDictionary<string, RateWindow> _rate = new();
    private const int DosThreshold = 100;
    private static readonly TimeSpan DosWindow = TimeSpan.FromSeconds(10);

    // Common SQL-injection signatures (heuristic — may occasionally false-positive).
    private static readonly Regex SqlInjectionPattern = new(
        @"(--)|(/\*)|(\bunion\b\s+\bselect\b)|(\bselect\b.+\bfrom\b)|(\binsert\b\s+\binto\b)|(\bdrop\b\s+\btable\b)|(\bor\b\s+1\s*=\s*1)|(\bxp_)|(\bexec\b\s*\()|(')",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SecurityAuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ISecurityAuditService audit)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (context.Request.Method == HttpMethods.Options || ShouldSkip(path))
        {
            await _next(context);
            return;
        }

        var ip = context.GetRealIpAddress();
        var ua = context.Request.Headers.UserAgent.ToString();
        var fullPath = path + context.Request.QueryString;

        // 1) SQL-injection heuristic on the (decoded) URL + query string.
        try
        {
            var decoded = Uri.UnescapeDataString(fullPath);
            if (SqlInjectionPattern.IsMatch(decoded))
            {
                await audit.LogAsync(SecurityEventTypes.SqlInjection, ipAddress: ip, userAgent: ua,
                    path: fullPath, detail: "Suspicious SQL pattern in request URL/query string.");
            }
        }
        catch { /* detection must never break the pipeline */ }

        // 2) Rate-based DoS heuristic (logged once when the threshold is crossed).
        try
        {
            if (ip is not null && RegisterAndCrossedThreshold(ip))
            {
                await audit.LogAsync(SecurityEventTypes.DosAttempt, ipAddress: ip, userAgent: ua,
                    path: fullPath, detail: $"Over {DosThreshold} requests within {DosWindow.TotalSeconds:N0}s from this IP.");
            }
        }
        catch { }

        await _next(context);

        // 3) Unauthorized / forbidden responses (login failures are logged in AuthController).
        try
        {
            var status = context.Response.StatusCode;
            if ((status == StatusCodes.Status401Unauthorized || status == StatusCodes.Status403Forbidden)
                && !path.Contains("/auth/login", StringComparison.OrdinalIgnoreCase))
            {
                await audit.LogAsync(SecurityEventTypes.Unauthorized, ipAddress: ip, userAgent: ua,
                    path: fullPath, detail: $"HTTP {status} on a protected resource.");
            }
        }
        catch { }
    }

    private static bool ShouldSkip(string path)
        => path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase)
        || path.Contains("/security", StringComparison.OrdinalIgnoreCase); // the audit dashboard itself

    private static bool RegisterAndCrossedThreshold(string ip)
    {
        var now = DateTime.UtcNow;
        var window = _rate.GetOrAdd(ip, _ => new RateWindow { Start = now, Count = 0 });
        lock (window)
        {
            if (now - window.Start > DosWindow)
            {
                window.Start = now;
                window.Count = 1;
                return false;
            }
            window.Count++;
            return window.Count == DosThreshold + 1; // fire exactly once per window
        }
    }

    private sealed class RateWindow
    {
        public DateTime Start;
        public int Count;
    }
}
