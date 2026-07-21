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
