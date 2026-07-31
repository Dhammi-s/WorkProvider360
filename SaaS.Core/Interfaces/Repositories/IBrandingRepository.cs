using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IBrandingRepository
{
    Task<Branding?> GetAsync(CancellationToken ct = default);
    Task<Branding> UpsertLogoAsync(string logoBase64, CancellationToken ct = default);
}
