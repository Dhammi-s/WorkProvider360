/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Entities;

/// <summary>
/// Single-row (SettingsId = 1) configuration for clients, service types, client
/// visits (auto clock, signatures, GPS) and the client portal, per tenant DB.
/// Access columns hold None | Read | Write.
/// </summary>
public sealed class ClientSettings
{
    public int SettingsId { get; set; }
    public string AdminClientAccess { get; set; } = "Write";
    public string ManagerClientAccess { get; set; } = "Read";
    public string AdminServiceTypeAccess { get; set; } = "Write";
    public string ManagerServiceTypeAccess { get; set; } = "Write";
    public bool AutoClockInEnabled { get; set; }
    public bool AutoClockOutEnabled { get; set; }
    public bool RequireClientSignatureOnClockIn { get; set; }
    public bool RequireClientSignatureOnClockOut { get; set; } = true;
    public bool RequireSameOffice { get; set; } = true;
    public bool RequireMatchingSkill { get; set; }
    public bool CaptureClockLocation { get; set; } = true;
    public bool ClientPortalEnabled { get; set; } = true;
    public bool SendClientCredentialsEmail { get; set; } = true;
    public bool NotifyClientOnSchedule { get; set; } = true;
    public bool RequireClientEmail { get; set; } = true;
    public bool RequireClientPhone { get; set; } = true;
    public bool RequireClientDateOfBirth { get; set; }
    public bool RequireEmergencyContact { get; set; }
    public bool RequireClientServiceTypes { get; set; } = true;
    public DateTime UpdatedOn { get; set; }
}
