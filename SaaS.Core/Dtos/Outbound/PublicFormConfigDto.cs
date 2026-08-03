/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>
/// Everything the anonymous application form needs to render: the roles that
/// can be requested, the active custom questions, and which standard fields
/// are mandatory.
/// </summary>
public sealed class PublicFormConfigDto
{
    public IReadOnlyList<RoleDto> Roles { get; set; } = new List<RoleDto>();
    public IReadOnlyList<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    public bool RequirePhone { get; set; }
    public bool RequireAddress { get; set; }
}
