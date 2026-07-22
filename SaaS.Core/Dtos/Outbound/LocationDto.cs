namespace SaaS.Core.Dtos.Outbound;

/// <summary>One recorded position in a schedule's breadcrumb trail.</summary>
public sealed class LocationPingDto
{
    public long PingId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? AccuracyMeters { get; set; }
    public DateTime RecordedUtc { get; set; }
}

/// <summary>Latest position of an active (clocked-in) schedule for the live map.</summary>
public sealed class LiveLocationDto
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Location { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? AccuracyMeters { get; set; }
    public DateTime RecordedUtc { get; set; }
}
