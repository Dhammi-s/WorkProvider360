/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Per-user assistant chat history (current TENANT database).</summary>
public interface IChatMessageRepository
{
    Task AddAsync(int userId, string role, string content, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessage>> GetByUserAsync(int userId, int take, CancellationToken ct = default);
    Task ClearByUserAsync(int userId, CancellationToken ct = default);
}
