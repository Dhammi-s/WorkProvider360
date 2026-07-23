using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface ITimezoneRepository
{
    Task<IReadOnlyList<Timezone>> GetActiveAsync(CancellationToken ct = default);
}
