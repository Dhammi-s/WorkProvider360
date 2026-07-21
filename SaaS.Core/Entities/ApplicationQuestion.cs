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
