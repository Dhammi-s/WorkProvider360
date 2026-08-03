/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// In-app notifications. Admins/SuperAdmins send a notification to a user; every
/// authenticated user reads their own bell and marks it read.
/// </summary>
[Authorize]
public sealed class NotificationsController : BaseApiController
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    /// <summary>Send an in-app notification to a user (SuperAdmin / Admin).</summary>
    [Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<object?>>> Send(
        [FromBody] SendNotificationRequestDto request, CancellationToken ct)
    {
        await _notifications.SendAsync(request, CurrentUserId, ct);
        return Ok(ApiResponse.Ok("Notification sent."));
    }

    /// <summary>The current user's notifications (newest first).</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> Mine(CancellationToken ct)
    {
        var items = await _notifications.GetForUserAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(items));
    }

    /// <summary>Unread count for the bell badge.</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> UnreadCount(CancellationToken ct)
    {
        var count = await _notifications.GetUnreadCountAsync(CurrentUserId, ct);
        return Ok(ApiResponse<int>.Ok(count));
    }

    /// <summary>Mark all of the current user's notifications as read.</summary>
    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<object?>>> ReadAll(CancellationToken ct)
    {
        await _notifications.MarkAllReadAsync(CurrentUserId, ct);
        return Ok(ApiResponse.Ok("Marked all as read."));
    }
}
