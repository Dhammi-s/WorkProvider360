using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>Role-application data access against the current TENANT database.</summary>
public interface IApplicationRepository
{
    Task<int> CreateAsync(RoleApplication application, CancellationToken ct = default);
    Task<IReadOnlyList<RoleApplication>> GetAllAsync(string? status, CancellationToken ct = default);
    Task<RoleApplication?> GetByIdAsync(int applicationId, CancellationToken ct = default);
    Task UpdateStatusAsync(int applicationId, string status, string? rejectionReason, int reviewedByUserId, CancellationToken ct = default);

    Task<int> CreateAnswerAsync(ApplicationAnswer answer, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationAnswer>> GetAnswersAsync(int applicationId, CancellationToken ct = default);
}
