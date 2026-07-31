using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface ILoginContentRepository
{
    Task<LoginPageContent?> GetAsync(CancellationToken ct = default);
    Task<LoginPageContent> UpsertAsync(LoginPageContent content, CancellationToken ct = default);
}
