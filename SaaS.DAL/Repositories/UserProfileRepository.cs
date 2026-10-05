/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Data;
using Dapper;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;

namespace SaaS.DAL.Repositories;

/// <summary>Staff profile, skills and availability against the current TENANT database.</summary>
public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserProfileRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<UserProfile?> GetAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleOrDefaultAsync<UserProfile>(
            new CommandDefinition("usp_UserProfile_Get", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<UserProfile> UpsertAsync(UserProfile p, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        return await db.QuerySingleAsync<UserProfile>(
            new CommandDefinition("usp_UserProfile_Upsert",
                new
                {
                    p.UserId,
                    p.AddressLine1,
                    p.AddressLine2,
                    p.City,
                    p.State,
                    p.PostalCode,
                    p.Country,
                    p.DateOfBirth,
                    p.Gender,
                    p.Qualifications,
                    p.YearsOfExperience,
                    p.About,
                    p.HasDrivingLicense,
                    p.HasVehicle,
                    p.EmergencyContactName,
                    p.EmergencyContactPhone,
                    p.HireDate,
                    p.ApplicationId
                },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task ReplaceServiceTypesAsync(int userId, string serviceTypeIdsJson, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_UserServiceType_Replace",
                new { UserId = userId, ServiceTypeIdsJson = serviceTypeIdsJson },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<ServiceType>> GetServiceTypesAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<ServiceType>(
            new CommandDefinition("usp_UserServiceType_GetByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task ReplaceAvailabilityAsync(int userId, string slotsJson, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        await db.ExecuteAsync(
            new CommandDefinition("usp_UserAvailability_Replace",
                new { UserId = userId, SlotsJson = slotsJson },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<AvailabilitySlot>> GetAvailabilityAsync(int userId, CancellationToken ct = default)
    {
        using var db = await _connectionFactory.CreateTenantConnectionAsync(ct);
        var rows = await db.QueryAsync<AvailabilitySlot>(
            new CommandDefinition("usp_UserAvailability_GetByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: ct));
        return rows.AsList();
    }
}
