using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class UpdateLogoRequestDto
{
    /// <summary>Base64 data URI, e.g. "data:image/png;base64,....".</summary>
    [Required]
    public string LogoBase64 { get; set; } = string.Empty;
}
