using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

/// <summary>
/// Reads tenant metadata from the MASTER database (Agencies table) via
/// stored procedures.
/// </summary>
public interface IAgencyRepository
{
    Task<Agency?> GetByDomainAsync(string domainUrl, CancellationToken ct = default);
    Task<Agency?> GetByIdAsync(int agencyId, CancellationToken ct = default);
}
