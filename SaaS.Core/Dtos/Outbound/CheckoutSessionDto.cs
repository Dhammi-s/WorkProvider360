namespace SaaS.Core.Dtos.Outbound;

public sealed class CheckoutSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
