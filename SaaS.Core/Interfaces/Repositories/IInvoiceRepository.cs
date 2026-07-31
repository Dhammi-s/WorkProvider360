/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Entities;

namespace SaaS.Core.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task<Guid> CreateAsync(Invoice invoice, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default);
}
