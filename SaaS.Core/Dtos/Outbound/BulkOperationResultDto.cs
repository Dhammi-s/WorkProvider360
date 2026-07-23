namespace SaaS.Core.Dtos.Outbound;

public sealed class BulkOperationResultDto
{
    public int Total { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
}
