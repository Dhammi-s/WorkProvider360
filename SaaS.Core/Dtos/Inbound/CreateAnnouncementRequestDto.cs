using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class CreateAnnouncementRequestDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateAnnouncementSettingsDto
{
    public bool ShowToAdmin { get; set; }
    public bool ShowToManager { get; set; }
    public bool ShowToUser { get; set; }
}
