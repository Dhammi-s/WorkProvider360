namespace SaaS.Core.Dtos.Outbound;

public sealed class ScheduleNoteDto
{
    public int NoteId { get; set; }
    public int ScheduleId { get; set; }
    public int AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string NoteType { get; set; } = "Note";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}
