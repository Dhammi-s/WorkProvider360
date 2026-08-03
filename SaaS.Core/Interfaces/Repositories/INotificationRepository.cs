/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-08-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface INotificationRepository
{
    Task CreateAsync(int userId, string? title, string message, int? createdByUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetByUserAsync(int userId, int take, CancellationToken ct = default);
    Task<int> UnreadCountAsync(int userId, CancellationToken ct = default);
    Task MarkAllReadAsync(int userId, CancellationToken ct = default);
}
