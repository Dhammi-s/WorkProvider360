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
