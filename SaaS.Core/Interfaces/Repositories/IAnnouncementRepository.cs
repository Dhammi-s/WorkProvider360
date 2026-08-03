/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IAnnouncementRepository
{
    Task<Guid> CreateAsync(string title, string message, int? createdByUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Announcement>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Announcement>> GetAllAsync(CancellationToken ct = default);
    Task DeactivateAsync(Guid announcementId, CancellationToken ct = default);

    Task<AnnouncementSettings?> GetSettingsAsync(CancellationToken ct = default);
    Task<AnnouncementSettings> UpsertSettingsAsync(AnnouncementSettings settings, CancellationToken ct = default);
}
