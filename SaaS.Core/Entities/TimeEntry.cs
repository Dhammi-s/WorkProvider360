/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A worked-time record for a schedule. Source is "Timer" (clock in/out) or
/// "Manual". ClockOutUtc is null while a timer is still running.
/// </summary>
public sealed class TimeEntry
{
    public int TimeEntryId { get; set; }
    public int ScheduleId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public string Source { get; set; } = "Timer";
    public string? Note { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
