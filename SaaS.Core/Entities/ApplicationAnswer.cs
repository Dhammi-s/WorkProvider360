namespace SaaS.Core.Entities;

/// <summary>
/// An applicant's answer to a custom question. The question text is snapshotted
/// so the record stays readable even if the question is later edited/removed.
/// </summary>
public sealed class ApplicationAnswer
{
    public int AnswerId { get; set; }
    public int ApplicationId { get; set; }
    public int? QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
}
