namespace SaaS.Core.Entities;

/// <summary>
/// A timezone reference row (shared master list, seeded into every tenant).
/// Offices point at one via <see cref="Office.TimezoneId"/>.
/// </summary>
public sealed class Timezone
{
    public Guid TimezoneId { get; set; }
    public string TimezoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
