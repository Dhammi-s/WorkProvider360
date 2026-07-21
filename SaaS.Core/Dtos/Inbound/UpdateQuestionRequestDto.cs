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
