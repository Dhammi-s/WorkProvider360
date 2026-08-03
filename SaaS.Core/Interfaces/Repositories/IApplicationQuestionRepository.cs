/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IApplicationQuestionRepository
{
    Task<IReadOnlyList<ApplicationQuestion>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationQuestion>> GetAllAsync(CancellationToken ct = default);
    Task<ApplicationQuestion?> GetByIdAsync(int questionId, CancellationToken ct = default);
    Task<int> CreateAsync(ApplicationQuestion question, CancellationToken ct = default);
    Task UpdateAsync(ApplicationQuestion question, CancellationToken ct = default);
    Task DeactivateAsync(int questionId, CancellationToken ct = default);
}
