/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A SuperAdmin-defined screening question shown on the public application form.
/// </summary>
public sealed class ApplicationQuestion
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedOn { get; set; }
}
