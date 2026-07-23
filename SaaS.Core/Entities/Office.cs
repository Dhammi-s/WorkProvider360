namespace SaaS.Core.Entities;

/// <summary>
/// An office belonging to the current tenant/agency. Staff are scoped to an
/// office and each office runs on its own timezone.
/// </summary>
public sealed class Office
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
    public DateTime? UpdatedOn { get; set; }
}
