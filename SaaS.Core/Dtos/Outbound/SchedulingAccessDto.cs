namespace SaaS.Core.Dtos.Outbound;

/// <summary>
/// The current user's effective scheduling permissions, so the client can show
/// the right controls. The server still enforces these on every request.
/// </summary>
public sealed class SchedulingAccessDto
{
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Effective access level: "None", "Read", "Write" (or "Self" for User).</summary>
    public string AccessLevel { get; set; } = "None";

    public bool IsSuperAdmin { get; set; }

    /// <summary>Can see all schedules (Admin/Manager with Read/Write, or SuperAdmin).</summary>
    public bool CanViewAll { get; set; }

    /// <summary>Can create/edit/delete/assign schedules and edit defaults.</summary>
    public bool CanManage { get; set; }

    /// <summary>Can set access levels (SuperAdmin only).</summary>
    public bool CanManageAccess { get; set; }

    /// <summary>Is a regular User who sees only their own assigned schedules.</summary>
    public bool IsSelfScoped { get; set; }
}
