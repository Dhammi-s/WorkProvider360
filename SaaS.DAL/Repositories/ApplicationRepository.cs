using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>
/// Role-application data access against the current TENANT database using
/// stored procedures.
/// </summary>
public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ApplicationRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<int> CreateAsync(RoleApplication application, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_RoleApplication_Create",
                new
                {
                    application.FullName,
                    application.Email,
                    application.Phone,
                    application.Address,
                    application.RequestedRoleId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<RoleApplication>> GetAllAsync(string? status, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<RoleApplication>(
            new CommandDefinition("usp_RoleApplication_GetAll", new { Status = status },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<RoleApplication?> GetByIdAsync(int applicationId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<RoleApplication>(
            new CommandDefinition("usp_RoleApplication_GetById", new { ApplicationId = applicationId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task UpdateStatusAsync(int applicationId, string status, string? rejectionReason, int reviewedByUserId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_RoleApplication_UpdateStatus",
                new
                {
                    ApplicationId = applicationId,
                    Status = status,
                    RejectionReason = rejectionReason,
                    ReviewedByUserId = reviewedByUserId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<int> CreateAnswerAsync(ApplicationAnswer answer, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition("usp_ApplicationAnswer_Create",
                new
                {
                    answer.ApplicationId,
                    answer.QuestionId,
                    answer.QuestionText,
                    answer.AnswerText
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ApplicationAnswer>> GetAnswersAsync(int applicationId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ApplicationAnswer>(
            new CommandDefinition("usp_ApplicationAnswer_GetByApplication", new { ApplicationId = applicationId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
