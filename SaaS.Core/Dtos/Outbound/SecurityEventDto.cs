namespace SaaS.Core.Dtos.Outbound;

public sealed class SecurityEventDto
{
    public Guid SecurityEventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Path { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>One row of the "events by type" breakdown.</summary>
public sealed class SecurityTypeCountDto
{
    public string EventType { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>Per-account login tallies.</summary>
public sealed class SecurityLoginStatDto
{
    public string Email { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}

/// <summary>Everything the SuperAdmin security dashboard needs in one payload.</summary>
public sealed class SecurityStatsDto
{
    public int TotalEvents { get; set; }
    public int TotalLogins { get; set; }
    public int FailedLogins { get; set; }
    public int Unauthorized { get; set; }
    public int SqlInjectionAttempts { get; set; }
    public int DosAttempts { get; set; }
    public IReadOnlyList<SecurityTypeCountDto> TypeCounts { get; set; } = new List<SecurityTypeCountDto>();
    public IReadOnlyList<SecurityLoginStatDto> LoginStats { get; set; } = new List<SecurityLoginStatDto>();
    public IReadOnlyList<SecurityEventDto> Recent { get; set; } = new List<SecurityEventDto>();
}
