namespace SaaS.Core.Dtos.Outbound;

public sealed class OfficeDto
{
    public Guid OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public Guid? TimezoneId { get; set; }
    public string? TimezoneName { get; set; }
    public bool IsActive { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedOn { get; set; }
}

public sealed class OfficeMemberDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
