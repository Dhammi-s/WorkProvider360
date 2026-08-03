/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IApplicationSettingsRepository
{
    Task<ApplicationSettings?> GetAsync(CancellationToken ct = default);
    Task<ApplicationSettings> UpsertAsync(ApplicationSettings settings, CancellationToken ct = default);
}
