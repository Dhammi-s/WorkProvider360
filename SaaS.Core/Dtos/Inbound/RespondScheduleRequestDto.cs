using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>The assigned user's response to a schedule: accept or reject.</summary>
public sealed class RespondScheduleRequestDto
{
    /// <summary>"Accept" or "Reject".</summary>
    [Required]
    public string Action { get; set; } = string.Empty;

    /// <summary>Reason, required when rejecting.</summary>
    public string? Reason { get; set; }
}
