namespace SaaS.Core.Dtos.Inbound;

public sealed class UpdateLogSettingsDto
{
    public bool AdminCanViewLogs { get; set; }
    public bool ManagerCanViewLogs { get; set; }
}
