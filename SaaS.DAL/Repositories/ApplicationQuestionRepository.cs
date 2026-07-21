using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

public sealed class ApplicationQuestionRepository : IApplicationQuestionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ApplicationQuestionRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<ApplicationQuestion>> GetActiveAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ApplicationQuestion>(
            new CommandDefinition("usp_ApplicationQuestion_GetActive",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<ApplicationQuestion>> GetAllAsync(CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ApplicationQuestion>(
            new CommandDefinition("usp_ApplicationQuestion_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<ApplicationQuestion?> GetByIdAsync(int questionId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<ApplicationQuestion>(
            new CommandDefinition("usp_ApplicationQuestion_GetById", new { QuestionId = questionId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> CreateAsync(ApplicationQuestion question, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_ApplicationQuestion_Create",
                new { question.QuestionText, question.IsRequired, question.SortOrder },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateAsync(ApplicationQuestion question, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_ApplicationQuestion_Update",
                new
                {
                    question.QuestionId,
                    question.QuestionText,
                    question.IsRequired,
                    question.IsActive,
                    question.SortOrder
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task DeactivateAsync(int questionId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_ApplicationQuestion_Deactivate", new { QuestionId = questionId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }
}
