/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
