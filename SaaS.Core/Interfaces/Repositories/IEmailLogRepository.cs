using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IEmailLogRepository
{
    Task CreateAsync(EmailLog log, CancellationToken ct = default);
    Task<IReadOnlyList<EmailLog>> GetRecentAsync(int top, CancellationToken ct = default);
    Task<(IReadOnlyList<EmailLog> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
