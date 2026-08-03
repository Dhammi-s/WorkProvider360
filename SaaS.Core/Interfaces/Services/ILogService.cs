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

/// <summary>
/// Email-log access. SuperAdmin always sees logs and manages the toggles; Admins
/// and Managers see logs only when the SuperAdmin has enabled it for their role.
/// </summary>
public interface ILogService
{
    Task<LogAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<EmailLogDto>> GetEmailLogsAsync(int roleId, int top, CancellationToken ct = default);
    Task<PagedResultDto<EmailLogDto>> GetEmailLogsPagedAsync(int roleId, int page, int pageSize, CancellationToken ct = default);
    Task<LogSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task<LogSettingsDto> UpdateSettingsAsync(UpdateLogSettingsDto request, CancellationToken ct = default);
}
