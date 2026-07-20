namespace SaaS.Core.Entities;

/// <summary>
/// Represents a tenant row from the master database "Agencies" table.
/// The <see cref="ConnectionString"/> points at that tenant's own database.
/// </summary>
public sealed class Agency
{
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string DomainUrl { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? DbServer { get; set; }
    public string? DbName { get; set; }
    public string? DbUser { get; set; }
    public string? DbPassword { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
