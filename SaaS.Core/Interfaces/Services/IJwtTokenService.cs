using System.Security.Claims;
using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Services;

public interface IJwtTokenService
{
    /// <summary>Builds a signed access token embedding agency id, user id and role.</summary>
    (string token, DateTime expiresOn) CreateAccessToken(int agencyId, AppUser user);

    /// <summary>Generates a cryptographically random opaque refresh token.</summary>
    string CreateRefreshToken();

    /// <summary>
    /// Reads the principal from an expired access token (signature validated,
    /// lifetime ignored) so the refresh flow can trust its claims.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
