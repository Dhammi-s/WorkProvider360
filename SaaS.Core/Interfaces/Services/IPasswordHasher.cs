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
