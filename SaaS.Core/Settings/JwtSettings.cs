namespace SaaS.Core.Settings;

/// <summary>
/// Strongly-typed JWT configuration bound from the "Jwt" section of appsettings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key. MUST be at least 64 bytes for HMAC-SHA512.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
