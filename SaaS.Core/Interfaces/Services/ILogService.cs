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
    Task<LogSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task<LogSettingsDto> UpdateSettingsAsync(UpdateLogSettingsDto request, CancellationToken ct = default);
}
