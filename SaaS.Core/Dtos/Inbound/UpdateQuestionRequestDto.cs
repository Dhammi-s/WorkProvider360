/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class UpdateQuestionRequestDto
{
    [Required, MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
