/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
