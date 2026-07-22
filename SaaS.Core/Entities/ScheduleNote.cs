namespace SaaS.Core.Entities;

/// <summary>
/// A note or injury report logged against a schedule. NoteType is "Note" or "Injury".
/// </summary>
public sealed class ScheduleNote
{
    public int NoteId { get; set; }
    public int ScheduleId { get; set; }
    public int AuthorUserId { get; set; }
    public string? AuthorName { get; set; }
    public string NoteType { get; set; } = "Note";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
