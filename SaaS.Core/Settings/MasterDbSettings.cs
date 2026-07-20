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
