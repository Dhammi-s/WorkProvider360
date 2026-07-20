using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Payload for creating the first SuperAdmin of a freshly provisioned tenant.
/// Only honored while the tenant has no users.
/// </summary>
public sealed class BootstrapAdminRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
