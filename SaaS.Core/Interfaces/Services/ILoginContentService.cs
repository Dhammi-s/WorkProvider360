/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

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
