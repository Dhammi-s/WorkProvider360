/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Inbound;

/// <summary>Update the client / visit / portal settings (SuperAdmin). All flags sent together.</summary>
public sealed class UpsertClientSettingsDto
{
    public string AdminClientAccess { get; set; } = "Write";
    public string ManagerClientAccess { get; set; } = "Read";
    public string AdminServiceTypeAccess { get; set; } = "Write";
    public string ManagerServiceTypeAccess { get; set; } = "Write";
    public bool AutoClockInEnabled { get; set; }
    public bool AutoClockOutEnabled { get; set; }
    public bool RequireClientSignatureOnClockIn { get; set; }
    public bool RequireClientSignatureOnClockOut { get; set; }
    public bool RequireSameOffice { get; set; }
    public bool RequireMatchingSkill { get; set; }
    public bool CaptureClockLocation { get; set; }
    public bool ClientPortalEnabled { get; set; }
    public bool SendClientCredentialsEmail { get; set; }
    public bool NotifyClientOnSchedule { get; set; }
    public bool RequireClientEmail { get; set; }
    public bool RequireClientPhone { get; set; }
    public bool RequireClientDateOfBirth { get; set; }
    public bool RequireEmergencyContact { get; set; }
    public bool RequireClientServiceTypes { get; set; }
}
