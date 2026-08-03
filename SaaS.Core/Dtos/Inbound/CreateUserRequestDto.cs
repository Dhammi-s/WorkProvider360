/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class CreateUserRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    /// <summary>Optional mobile number for SMS notifications, e.g. "+15551234567".</summary>
    [Phone]
    public string? Phone { get; set; }

    /// <summary>Office the user belongs to. Optional for SuperAdmin-created accounts.</summary>
    public Guid? OfficeId { get; set; }

    /// <summary>Salary for Admin/Manager accounts (used by the accounting/payroll flow).</summary>
    public decimal? Salary { get; set; }
}
