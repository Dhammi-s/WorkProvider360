using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>A cropped profile image as a base64 data URI (data:image/...;base64,...).</summary>
public sealed class UpdateAvatarRequestDto
{
    [Required]
    public string ImageBase64 { get; set; } = string.Empty;
}
