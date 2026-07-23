namespace SaaS.Core.Dtos.Outbound;

public sealed class EmailLogDto
{
    public Guid EmailLogId { get; set; }
    public string ToAddress { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>Whether the current user may view logs (and manage the toggles).</summary>
public sealed class LogAccessDto
{
    public bool CanView { get; set; }
    public bool CanManageAccess { get; set; }
}

public sealed class LogSettingsDto
{
    public bool AdminCanViewLogs { get; set; }
    public bool ManagerCanViewLogs { get; set; }
    public DateTime UpdatedOn { get; set; }
}
