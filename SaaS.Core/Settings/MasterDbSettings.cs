/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Settings;

/// <summary>
/// Holds the master database connection string. The master DB contains the
/// Agencies table used to resolve each tenant's own connection string.
/// </summary>
public sealed class MasterDbSettings
{
    public const string SectionName = "MasterDb";

    public string ConnectionString { get; set; } = string.Empty;
}
