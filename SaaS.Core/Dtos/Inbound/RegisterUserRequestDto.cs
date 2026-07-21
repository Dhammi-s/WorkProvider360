using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Public self-registration payload; always creates a "User" role account.</summary>
public sealed class RegisterUserRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
