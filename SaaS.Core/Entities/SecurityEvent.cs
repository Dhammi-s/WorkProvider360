namespace SaaS.Core.Entities;

/// <summary>A tenant-scoped security audit record (login attempt or detected attack).</summary>
public sealed class SecurityEvent
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
