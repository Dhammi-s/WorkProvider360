/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Salted SHA-512 password hashing.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Creates a new random salt and returns the SHA-512 hash of (salt + password).</summary>
    (string hash, string salt) HashPassword(string password);

    /// <summary>Verifies a plaintext password against a stored hash + salt.</summary>
    bool Verify(string password, string storedHash, string storedSalt);
}
