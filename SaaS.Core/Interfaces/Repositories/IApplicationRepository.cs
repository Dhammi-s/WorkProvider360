/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Role-application data access against the current TENANT database.</summary>
public interface IApplicationRepository
{
    Task<int> CreateAsync(RoleApplication application, CancellationToken ct = default);
    Task<IReadOnlyList<RoleApplication>> GetAllAsync(string? status, CancellationToken ct = default);
    Task<(IReadOnlyList<RoleApplication> Items, int Total)> GetPagedAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task<RoleApplication?> GetByIdAsync(int applicationId, CancellationToken ct = default);
    Task UpdateStatusAsync(int applicationId, string status, string? rejectionReason, int reviewedByUserId, CancellationToken ct = default);

    Task<int> CreateAnswerAsync(ApplicationAnswer answer, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationAnswer>> GetAnswersAsync(int applicationId, CancellationToken ct = default);
}
