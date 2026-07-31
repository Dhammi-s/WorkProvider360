/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A refresh token persisted in the tenant database. Used to issue new access
/// tokens without re-authenticating.
/// </summary>
public sealed class RefreshToken
{
    public long RefreshTokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedOn { get; set; }
}
