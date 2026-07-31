/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// Single-row (SettingsId = 1) config controlling who may view the email logs.
/// SuperAdmin always has access, regardless of these flags.
/// </summary>
public sealed class LogSettings
{
    public int SettingsId { get; set; }
    public bool AdminCanViewLogs { get; set; }
    public bool ManagerCanViewLogs { get; set; }
    public DateTime UpdatedOn { get; set; }
}
