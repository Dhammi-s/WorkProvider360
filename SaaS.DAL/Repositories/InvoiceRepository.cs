using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateAsync(Invoice invoice, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<Guid>(
            new CommandDefinition("usp_Invoice_Create",
                new
                {
                    invoice.InvoiceNumber,
                    invoice.RecipientUserId,
                    invoice.RecipientName,
                    invoice.RecipientEmail,
                    invoice.RecipientRoleName,
                    invoice.InvoiceType,
                    invoice.Amount,
                    invoice.RegularHours,
                    invoice.OvertimeHours,
                    invoice.TotalHours,
                    invoice.PeriodFrom,
                    invoice.PeriodTo,
                    invoice.Details,
                    invoice.PdfBase64,
                    invoice.PaymentMethod,
                    invoice.CreatedByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<Invoice>(
            new CommandDefinition("usp_Invoice_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<Invoice>(
            new CommandDefinition("usp_Invoice_GetById", new { InvoiceId = invoiceId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
