namespace SaaS.Core.Dtos.Outbound;

public sealed class TimezoneDto
{
    public Guid TimezoneId { get; set; }
    public string TimezoneName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
