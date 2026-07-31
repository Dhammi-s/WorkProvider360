namespace WebApplication1.Infrastructure;

/// <summary>Helpers for pulling the real client IP from a (possibly proxied) request.</summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// The real originating client IP. Behind the runasp.net proxy the actual
    /// client sits in <c>X-Forwarded-For</c> (first hop); we fall back to
    /// <c>X-Real-IP</c> and finally the raw connection address.
    /// </summary>
    public static string? GetRealIpAddress(this HttpContext context)
    {
        var headers = context.Request.Headers;

        var forwarded = headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(first)) return first;
        }

        var realIp = headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp)) return realIp.Trim();

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
