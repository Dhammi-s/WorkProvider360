/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-09
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// Single-row per-tenant configuration for the office meeting feature.
/// AdminAccess / ManagerAccess are "None", "Read" or "Write".
/// SuperAdmin is always full-write regardless of these settings.
/// </summary>
public sealed class MeetingSettings
{
    public int SettingsId { get; set; }

    /// <summary>"None" | "Read" | "Write"</summary>
    public string AdminAccess { get; set; } = "Write";

    /// <summary>"None" | "Read" | "Write"</summary>
    public string ManagerAccess { get; set; } = "Write";

    /// <summary>When true, Users (role 4) may create meetings themselves.</summary>
    public bool UserCanCreate { get; set; }

    /// <summary>Allow clients to be added as meeting participants.</summary>
    public bool AllowClientParticipants { get; set; } = true;

    /// <summary>Allow meetings to have a fee charged to participants.</summary>
    public bool AllowPaidMeetings { get; set; }

    public decimal DefaultFeePerParticipant { get; set; }

    /// <summary>Non-SuperAdmin meeting creations need approval before they appear.</summary>
    public bool RequireApproval { get; set; }

    public bool NotifyOnCreate { get; set; } = true;
    public bool NotifyOnUpdate { get; set; }
    public bool NotifyOnCancel { get; set; } = true;

    public int MaxParticipantsDefault { get; set; } = 50;

    public DateTime UpdatedOn { get; set; }
}
