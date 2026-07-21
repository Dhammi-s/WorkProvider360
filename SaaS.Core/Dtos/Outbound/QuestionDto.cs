namespace SaaS.Core.Dtos.Outbound;

public sealed class QuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
