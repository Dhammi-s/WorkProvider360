namespace SaaS.Core.Entities;

/// <summary>
/// A role row from a tenant database "Roles" table. Role ids are static and
/// shared across all tenants (see <see cref="Constants.RoleConstants"/>).
/// </summary>
public sealed class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
