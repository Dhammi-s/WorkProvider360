namespace SaaS.Core.Entities;

/// <summary>
/// A submitted request for elevated (Admin/Manager) access, awaiting review.
/// </summary>
public sealed class RoleApplication
{
    public int ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int RequestedRoleId { get; set; }
    public string? RequestedRoleName { get; set; }
    public decimal? DesiredSalary { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectionReason { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedOn { get; set; }
    public DateTime CreatedOn { get; set; }
}
