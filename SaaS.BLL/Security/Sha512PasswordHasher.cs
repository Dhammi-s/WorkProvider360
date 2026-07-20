using System.Security.Cryptography;
using System.Text;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Security;

/// <summary>
/// Salted SHA-512 password hasher.
///
/// NOTE: SHA-512 is a fast hash and, even salted, is weaker than a purpose-built
/// password KDF (PBKDF2 / bcrypt / Argon2) against offline brute-force. This
/// implementation was requested explicitly; if you can, prefer
/// <c>Rfc2898DeriveBytes</c> (PBKDF2-SHA512) with a high iteration count.
/// </summary>
public sealed class Sha512PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 32;

    public (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = ComputeHash(password, saltBytes);
        return (hash, salt);
    }

    public bool Verify(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
            return false;

        byte[] saltBytes;
        try
        {
            saltBytes = Convert.FromBase64String(storedSalt);
        }
        catch (FormatException)
        {
            return false;
        }

        var computed = ComputeHash(password, saltBytes);

        // Constant-time comparison to avoid timing attacks.
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(storedHash));
    }

    private static string ComputeHash(string password, byte[] saltBytes)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combined = new byte[saltBytes.Length + passwordBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

        var hashBytes = SHA512.HashData(combined);
        return Convert.ToBase64String(hashBytes);
    }
}
