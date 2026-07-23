namespace SaaS.Core.Dtos.Inbound;

/// <summary>
/// Optional payload when approving an application. A SuperAdmin supplies the
/// office to place the new user in; an Admin approver's own office is used
/// automatically (this value is ignored for Admins).
/// </summary>
public sealed class ApproveApplicationRequestDto
{
    public Guid? OfficeId { get; set; }
}
