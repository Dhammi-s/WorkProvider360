/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Constants;

/// <summary>
/// Static, well-known role identifiers. These IDs are fixed across every tenant
/// database and match the seeded rows in the Roles table (see /sql scripts).
/// Keep these in sync with the database seed script.
/// </summary>
public static class RoleConstants
{
    public const int SuperAdminId = 1;
    public const int AdminId = 2;
    public const int ManagerId = 3;
    public const int UserId = 4;

    /// <summary>Portal-only role for clients whose visits the agency serves.</summary>
    public const int ClientId = 5;

    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Client = "Client";
}
