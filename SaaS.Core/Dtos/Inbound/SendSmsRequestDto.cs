using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Manual SMS send. Provide either a <see cref="UserId"/> (uses that user's
/// stored phone) or an explicit <see cref="ToNumber"/>.
/// </summary>
public sealed class SendSmsRequestDto
{
    /// <summary>Existing user to text; their stored phone number is used.</summary>
    public int? UserId { get; set; }

    /// <summary>Explicit destination number (used when no UserId, or the user has no phone on file).</summary>
    [Phone]
    public string? ToNumber { get; set; }

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
