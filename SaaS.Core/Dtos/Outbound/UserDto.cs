namespace SaaS.Core.Dtos.Outbound;

/// <summary>
/// Safe, outward-facing projection of a user (never exposes hash/salt).
/// </summary>
public sealed class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public decimal? Salary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}
