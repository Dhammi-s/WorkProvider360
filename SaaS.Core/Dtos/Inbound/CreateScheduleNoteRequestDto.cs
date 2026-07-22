using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Add a note or injury report to a schedule.</summary>
public sealed class CreateScheduleNoteRequestDto
{
    /// <summary>"Note" or "Injury".</summary>
    public string NoteType { get; set; } = "Note";

    [Required]
    public string Message { get; set; } = string.Empty;
}
