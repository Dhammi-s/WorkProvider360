namespace SaaS.Core.Entities;

/// <summary>
/// A single GPS position recorded by the assigned user's device while they are
/// clocked in on a schedule.
/// </summary>
public sealed class LocationPing
{
    public long PingId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? AccuracyMeters { get; set; }
    public DateTime RecordedUtc { get; set; }
}

/// <summary>
/// Latest known position of a currently-active (clocked-in) schedule, for the
/// live map overview.
/// </summary>
public sealed class LiveLocation
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
