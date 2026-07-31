using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Guid> CreateAsync(Invoice invoice, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default);
}
