using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Manually add or correct a worked-time record (UTC times).</summary>
public sealed class ManualTimeEntryRequestDto
{
    [Required]
    public DateTime ClockInUtc { get; set; }

    [Required]
    public DateTime ClockOutUtc { get; set; }

    public string? Note { get; set; }
}
