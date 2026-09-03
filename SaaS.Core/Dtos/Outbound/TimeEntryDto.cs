/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

public sealed class TimeEntryDto
{
    public int TimeEntryId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public decimal? ClockInLatitude { get; set; }
    public decimal? ClockInLongitude { get; set; }
    public decimal? ClockOutLatitude { get; set; }
    public decimal? ClockOutLongitude { get; set; }
    public bool HasClockInSignature { get; set; }
    public bool HasClockOutSignature { get; set; }
    public string Source { get; set; } = "Timer";
    public string? Note { get; set; }

    /// <summary>Worked hours for this entry; 0 while the timer is still running.</summary>
    public decimal Hours { get; set; }
}
