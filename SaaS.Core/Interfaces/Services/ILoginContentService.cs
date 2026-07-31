using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

public interface ILoginContentService
{
    /// <summary>Public login-page payload: agency name + logo + content (with defaults).</summary>
    Task<PublicLoginPageDto> GetPublicAsync(CancellationToken ct = default);

    /// <summary>The editable content (with defaults filled in) for the SuperAdmin editor.</summary>
    Task<LoginContentDto> GetForEditAsync(CancellationToken ct = default);

    /// <summary>SuperAdmin: save the login-page content.</summary>
    Task<LoginContentDto> UpdateAsync(UpdateLoginContentDto request, CancellationToken ct = default);
}
