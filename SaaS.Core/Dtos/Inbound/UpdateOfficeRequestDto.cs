using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class UpdateOfficeRequestDto
{
    [Required, MaxLength(200)]
    public string OfficeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    public Guid? TimezoneId { get; set; }

    public bool IsActive { get; set; } = true;
}
