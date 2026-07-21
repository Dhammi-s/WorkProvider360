using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class RejectApplicationRequestDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
