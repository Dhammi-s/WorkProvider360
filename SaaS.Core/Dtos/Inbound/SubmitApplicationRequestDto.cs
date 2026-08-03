/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Anonymous submission requesting Admin/Manager access.</summary>
public sealed class SubmitApplicationRequestDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    [Required]
    public int RequestedRoleId { get; set; }

    /// <summary>Applicant's expected salary based on experience (optional).</summary>
    public decimal? DesiredSalary { get; set; }

    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public sealed class SubmitAnswerDto
{
    [Required]
    public int QuestionId { get; set; }

    public string? AnswerText { get; set; }
}
