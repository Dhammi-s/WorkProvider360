using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _repo;

    public AnnouncementService(IAnnouncementRepository repo) => _repo = repo;

    public async Task<AnnouncementViewDto> GetForViewerAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _repo.GetSettingsAsync(ct);
        var canManage = roleId == RoleConstants.SuperAdminId;
        var canView = canManage || CanRoleView(roleId, settings);

        // SuperAdmin sees all (incl. inactive) for management; others see active only.
        IReadOnlyList<Announcement> list = canView
            ? (canManage ? await _repo.GetAllAsync(ct) : await _repo.GetActiveAsync(ct))
            : new List<Announcement>();

        return new AnnouncementViewDto
        {
            CanView = canView,
            CanManage = canManage,
            Announcements = list.Select(Map).ToList(),
        };
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequestDto request, int createdByUserId, CancellationToken ct = default)
    {
        var id = await _repo.CreateAsync(request.Title, request.Message, createdByUserId, ct);
        return new AnnouncementDto
        {
            AnnouncementId = id,
            Title = request.Title,
            Message = request.Message,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
        };
    }

    public Task DeactivateAsync(Guid announcementId, CancellationToken ct = default)
        => _repo.DeactivateAsync(announcementId, ct);

    public async Task<AnnouncementSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var s = await _repo.GetSettingsAsync(ct);
        return Map(s);
    }

    public async Task<AnnouncementSettingsDto> UpdateSettingsAsync(UpdateAnnouncementSettingsDto request, CancellationToken ct = default)
    {
        var saved = await _repo.UpsertSettingsAsync(new AnnouncementSettings
        {
            ShowToAdmin = request.ShowToAdmin,
            ShowToManager = request.ShowToManager,
            ShowToUser = request.ShowToUser,
        }, ct);
        return Map(saved);
    }

    private static bool CanRoleView(int roleId, AnnouncementSettings? s) => roleId switch
    {
        RoleConstants.AdminId => s?.ShowToAdmin ?? true,
        RoleConstants.ManagerId => s?.ShowToManager ?? true,
        RoleConstants.UserId => s?.ShowToUser ?? true,
        _ => false,
    };

    private static AnnouncementDto Map(Announcement a) => new()
    {
        AnnouncementId = a.AnnouncementId,
        Title = a.Title,
        Message = a.Message,
        IsActive = a.IsActive,
        CreatedOn = a.CreatedOn,
    };

    private static AnnouncementSettingsDto Map(AnnouncementSettings? s) => new()
    {
        ShowToAdmin = s?.ShowToAdmin ?? true,
        ShowToManager = s?.ShowToManager ?? true,
        ShowToUser = s?.ShowToUser ?? true,
        UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
    };
}
