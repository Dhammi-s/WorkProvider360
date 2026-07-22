using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Edit an existing schedule. Times are UTC.</summary>
public sealed class UpdateScheduleRequestDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? Location { get; set; }

    [Required]
    public int AssignedUserId { get; set; }

    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    [Range(0, 100000)]
    public decimal PayRatePerHour { get; set; }

    [Range(1, 10)]
    public decimal OvertimeMultiplier { get; set; } = 1.5m;

    public string? ColorTag { get; set; }
}
