using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class LogService : ILogService
{
    private const int MaxLogs = 500;

    private readonly IEmailLogRepository _emailLog;
    private readonly ILogSettingsRepository _settings;

    public LogService(IEmailLogRepository emailLog, ILogSettingsRepository settings)
    {
        _emailLog = emailLog;
        _settings = settings;
    }

    public async Task<LogAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        return new LogAccessDto
        {
            CanView = CanView(roleId, settings),
            CanManageAccess = roleId == RoleConstants.SuperAdminId,
        };
    }

    public async Task<IReadOnlyList<EmailLogDto>> GetEmailLogsAsync(int roleId, int top, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        if (!CanView(roleId, settings))
            throw AppException.Forbidden("You do not have access to the logs.");

        var logs = await _emailLog.GetRecentAsync(Math.Clamp(top, 1, MaxLogs), ct);
        return logs.Select(l => new EmailLogDto
        {
            EmailLogId = l.EmailLogId,
            ToAddress = l.ToAddress,
            Subject = l.Subject,
            Body = l.Body,
            Status = l.Status,
            ErrorMessage = l.ErrorMessage,
            CreatedOn = l.CreatedOn,
        }).ToList();
    }

    public async Task<LogSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var s = await _settings.GetAsync(ct);
        return Map(s);
    }

    public async Task<LogSettingsDto> UpdateSettingsAsync(UpdateLogSettingsDto request, CancellationToken ct = default)
    {
        var saved = await _settings.UpsertAsync(new LogSettings
        {
            AdminCanViewLogs = request.AdminCanViewLogs,
            ManagerCanViewLogs = request.ManagerCanViewLogs,
        }, ct);
        return Map(saved);
    }

    private static bool CanView(int roleId, LogSettings? settings) => roleId switch
    {
        RoleConstants.SuperAdminId => true,
        RoleConstants.AdminId => settings?.AdminCanViewLogs ?? false,
        RoleConstants.ManagerId => settings?.ManagerCanViewLogs ?? false,
        _ => false,
    };

    private static LogSettingsDto Map(LogSettings? s) => new()
    {
        AdminCanViewLogs = s?.AdminCanViewLogs ?? false,
        ManagerCanViewLogs = s?.ManagerCanViewLogs ?? false,
        UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
    };
}
