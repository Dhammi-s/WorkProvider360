namespace SaaS.Core.Dtos.Outbound;

public sealed class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
