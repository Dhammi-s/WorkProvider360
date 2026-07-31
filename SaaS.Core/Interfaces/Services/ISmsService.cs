/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Interfaces.Services;

/// <summary>Sends SMS to users via Twilio.</summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS. Returns the provider message SID on success. Throws
    /// <see cref="SaaS.Core.Exceptions.AppException"/> if Twilio is not configured
    /// or the send is rejected.
    /// </summary>
    Task<string> SendAsync(string toNumber, string message, CancellationToken ct = default);
}
