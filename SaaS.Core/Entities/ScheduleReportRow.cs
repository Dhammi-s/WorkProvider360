/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// A single schedule's raw report row as returned by usp_Schedule_GetReport.
/// The BLL layer derives regular/overtime hours and earnings from these values.
/// </summary>
public sealed class ScheduleReportRow
{
    public int ScheduleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AssignedUserId { get; set; }
    public string AssignedUserName { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public decimal PayRatePerHour { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public string Status { get; set; } = string.Empty;
    public long WorkedSeconds { get; set; }
}
