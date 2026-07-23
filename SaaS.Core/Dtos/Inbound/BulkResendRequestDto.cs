using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class BulkResendRequestDto
{
    [Required, MinLength(1)]
    public List<int> UserIds { get; set; } = new();
}
