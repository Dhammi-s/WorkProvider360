namespace SaaS.Core.Constants;

/// <summary>
/// Custom JWT claim type names used across the application. The agency id and
/// user id travel inside every access token so the base controller can resolve
/// the current tenant and user without another round-trip.
/// </summary>
public static class AppClaimTypes
{
    public const string AgencyId = "agency_id";
    public const string UserId = "user_id";
    public const string RoleId = "role_id";
    public const string RoleName = "role_name";
    public const string Email = "email";
    public const string FullName = "full_name";
}
