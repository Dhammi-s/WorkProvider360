/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserService _users;

    public NotificationService(INotificationRepository notifications, IUserService users)
    {
        _notifications = notifications;
        _users = users;
    }

    public async Task SendAsync(SendNotificationRequestDto request, int senderUserId, CancellationToken ct = default)
    {
        var message = (request.Message ?? string.Empty).Trim();
        if (message.Length == 0)
            throw AppException.BadRequest("The notification message cannot be empty.");

        var recipient = await _users.GetByIdAsync(request.UserId, ct)
            ?? throw AppException.NotFound("The selected user does not exist.");

        await _notifications.CreateAsync(recipient.UserId, Clean(request.Title), message, senderUserId, ct);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(int userId, CancellationToken ct = default)
    {
        var rows = await _notifications.GetByUserAsync(userId, 50, ct);
        return rows.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedByName = n.CreatedByName,
            CreatedOn = n.CreatedOn,
        }).ToList();
    }

    public Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
        => _notifications.UnreadCountAsync(userId, ct);

    public Task MarkAllReadAsync(int userId, CancellationToken ct = default)
        => _notifications.MarkAllReadAsync(userId, ct);

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
