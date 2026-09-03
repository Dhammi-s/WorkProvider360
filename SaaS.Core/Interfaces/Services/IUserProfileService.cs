/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>Staff profile + skills + availability. A user manages their own; managers manage staff below them.</summary>
public interface IUserProfileService
{
    Task<UserProfileDto> GetAsync(int targetUserId, int currentUserId, int roleId, CancellationToken ct = default);
    Task<UserProfileDto> UpsertAsync(int targetUserId, UpsertUserProfileRequestDto request, int currentUserId, int roleId, CancellationToken ct = default);
}
