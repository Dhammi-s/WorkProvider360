namespace SaaS.Core.Entities;

/// <summary>
/// Single-row (SettingsId = 1) tenant branding. The agency logo is stored as a
/// base64 data URI (no cloud/blob storage).
/// </summary>
public sealed class Branding
{
    public int SettingsId { get; set; }
    public string? LogoBase64 { get; set; }
    public DateTime UpdatedOn { get; set; }
}
