/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>Row shown in the SuperAdmin applications list.</summary>
public sealed class ApplicationListItemDto
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RequestedRoleId { get; set; }
    public string RequestedRoleName { get; set; } = string.Empty;
    public decimal? DesiredSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

/// <summary>Full application detail incl. answers, for the review screen / PDF.</summary>
public sealed class ApplicationDetailDto
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int RequestedRoleId { get; set; }
    public string RequestedRoleName { get; set; } = string.Empty;
    public decimal? DesiredSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public IReadOnlyList<ApplicationAnswerDto> Answers { get; set; } = new List<ApplicationAnswerDto>();
}

public sealed class ApplicationAnswerDto
{
    public int? QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
}
