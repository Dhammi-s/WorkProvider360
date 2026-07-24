namespace SaaS.Core.Constants;

/// <summary>Well-known values for <c>SecurityEvent.EventType</c>.</summary>
public static class SecurityEventTypes
{
    public const string LoginSuccess = "LoginSuccess";
    public const string LoginFailed = "LoginFailed";
    public const string Unauthorized = "Unauthorized";
    public const string SqlInjection = "SqlInjection";
    public const string DosAttempt = "DosAttempt";
}
