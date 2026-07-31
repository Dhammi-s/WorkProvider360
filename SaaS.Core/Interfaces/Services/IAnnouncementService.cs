using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Announcements. SuperAdmin manages them and controls which roles see them;
/// Admins/Managers/Users see active announcements when their role is enabled.
/// </summary>
public interface IAnnouncementService
{
    Task<AnnouncementViewDto> GetForViewerAsync(int roleId, CancellationToken ct = default);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequestDto request, int createdByUserId, CancellationToken ct = default);
    Task DeactivateAsync(Guid announcementId, CancellationToken ct = default);

    Task<AnnouncementSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task<AnnouncementSettingsDto> UpdateSettingsAsync(UpdateAnnouncementSettingsDto request, CancellationToken ct = default);
}
