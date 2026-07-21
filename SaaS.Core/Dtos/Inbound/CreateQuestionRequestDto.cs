using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

public sealed class CreateQuestionRequestDto
{
    [Required, MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = true;

    public int SortOrder { get; set; }
}
