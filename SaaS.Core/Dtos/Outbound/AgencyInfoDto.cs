namespace SaaS.Core.Dtos.Outbound;

/// <summary>Safe, public projection of the current tenant (no connection secrets).</summary>
public sealed class AgencyInfoDto
{
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string? Location { get; set; }
}
