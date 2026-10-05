/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Read + limited-write endpoints for a signed-in client. The Users row (Client
/// role) is linked from Clients.UserId; every method resolves the client from
/// the caller user id and never trusts a client id from the request.
/// </summary>
public sealed class ClientPortalService : IClientPortalService
{
    private readonly IClientRepository _clients;
    private readonly IScheduleRepository _schedules;

    public ClientPortalService(IClientRepository clients, IScheduleRepository schedules)
    {
        _clients = clients;
        _schedules = schedules;
    }

    public async Task<PortalProfileDto> GetMeAsync(int userId, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);
        var services = await _clients.GetServiceTypesAsync(client.ClientId, ct);
        return MapProfile(client, services);
    }

    public async Task<PortalProfileDto> UpdateMeAsync(int userId, UpdatePortalProfileRequestDto request, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);

        client.Phone = Clean(request.Phone);
        client.AlternatePhone = Clean(request.AlternatePhone);
        client.AccessInstructions = Clean(request.AccessInstructions);
        client.EmergencyContactName = Clean(request.EmergencyContactName);
        client.EmergencyContactPhone = Clean(request.EmergencyContactPhone);
        client.EmergencyContactRelation = Clean(request.EmergencyContactRelation);
        client.PreferredLanguage = Clean(request.PreferredLanguage);

        await _clients.UpdateAsync(client, ct);

        var services = await _clients.GetServiceTypesAsync(client.ClientId, ct);
        return MapProfile(client, services);
    }

    public async Task<PortalDashboardDto> GetDashboardAsync(int userId, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);
        var visits = await _schedules.GetByClientAsync(client.ClientId, null, null, null, ct);
        var services = await _clients.GetServiceTypesAsync(client.ClientId, ct);
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var upcoming = visits
            .Where(v => v.StartUtc >= now && v.Status is not ("Completed" or "Cancelled" or "Rejected"))
            .OrderBy(v => v.StartUtc)
            .ToList();

        var today = visits
            .Where(v => v.StartUtc.Date == now.Date)
            .OrderBy(v => v.StartUtc)
            .Select(MapVisit)
            .ToList();

        var totalSeconds = visits.Where(v => v.StartUtc >= monthStart).Sum(v => v.WorkedSeconds);

        return new PortalDashboardDto
        {
            NextVisit = upcoming.Count > 0 ? MapVisit(upcoming[0]) : null,
            TodayVisits = today,
            UpcomingCount = upcoming.Count,
            CompletedCount = visits.Count(v => v.Status == "Completed"),
            TotalHoursThisMonth = Math.Round((decimal)totalSeconds / 3600m, 2, MidpointRounding.AwayFromZero),
            Services = services.Select(MapService).ToList(),
        };
    }

    public async Task<IReadOnlyList<ClientVisitDto>> GetVisitsAsync(int userId, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);
        var rows = await _schedules.GetByClientAsync(client.ClientId, fromUtc, toUtc, Clean(status), ct);
        return rows.Select(MapVisit).ToList();
    }

    public async Task<ClientVisitDetailDto> GetVisitAsync(int userId, int scheduleId, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);
        var visits = await _schedules.GetByClientAsync(client.ClientId, null, null, null, ct);
        var visit = visits.FirstOrDefault(v => v.ScheduleId == scheduleId)
            ?? throw AppException.NotFound("Visit not found.");

        var entries = await _schedules.GetTimeEntriesAsync(scheduleId, ct);
        var signatures = new List<TimeEntrySignatureDto>();
        foreach (var e in entries)
        {
            var sigs = await _schedules.GetSignaturesAsync(e.TimeEntryId, ct);
            signatures.AddRange(sigs.Select(MapSignature));
        }

        var detail = new ClientVisitDetailDto
        {
            TimeEntries = entries.Select(MapTime).ToList(),
            Signatures = signatures,
        };
        CopyVisit(detail, visit);
        return detail;
    }

    public async Task<IReadOnlyList<ServiceTypeDto>> GetServiceTypesAsync(int userId, CancellationToken ct = default)
    {
        var client = await RequireClientAsync(userId, ct);
        var services = await _clients.GetServiceTypesAsync(client.ClientId, ct);
        return services.Select(MapService).ToList();
    }

    // ------------------------------------------------------------------- Helpers

    private async Task<Client> RequireClientAsync(int userId, CancellationToken ct)
    {
        var client = await _clients.GetByUserIdAsync(userId, ct)
            ?? throw AppException.Forbidden("No client profile is linked to this account.");
        if (!client.PortalEnabled)
            throw AppException.Forbidden("Portal access is disabled for your account.");
        return client;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ClientVisitDto MapVisit(ClientVisit v)
    {
        var dto = new ClientVisitDto();
        CopyVisit(dto, v);
        return dto;
    }

    private static void CopyVisit(ClientVisitDto dto, ClientVisit v)
    {
        dto.ScheduleId = v.ScheduleId;
        dto.Title = v.Title;
        dto.ClientId = v.ClientId;
        dto.ServiceTypeId = v.ServiceTypeId;
        dto.ServiceTypeName = v.ServiceTypeName;
        dto.AssignedUserId = v.AssignedUserId;
        dto.AssignedUserName = v.AssignedUserName;
        dto.AssignedUserAvatarUrl = v.AssignedUserAvatarUrl;
        dto.Location = v.Location;
        dto.StartUtc = v.StartUtc;
        dto.EndUtc = v.EndUtc;
        dto.Status = v.Status;
        dto.ClockInUtc = v.ClockInUtc;
        dto.ClockOutUtc = v.ClockOutUtc;
        dto.WorkedSeconds = v.WorkedSeconds;
        dto.HasClockOutSignature = v.HasClockOutSignature;
    }

    private static TimeEntryDto MapTime(TimeEntry t) => new()
    {
        TimeEntryId = t.TimeEntryId,
        ScheduleId = t.ScheduleId,
        UserId = t.UserId,
        UserName = t.UserName ?? string.Empty,
        ClockInUtc = t.ClockInUtc,
        ClockOutUtc = t.ClockOutUtc,
        ClockInLatitude = t.ClockInLatitude,
        ClockInLongitude = t.ClockInLongitude,
        ClockOutLatitude = t.ClockOutLatitude,
        ClockOutLongitude = t.ClockOutLongitude,
        HasClockInSignature = t.HasClockInSignature,
        HasClockOutSignature = t.HasClockOutSignature,
        Source = t.Source,
        Note = t.Note,
        Hours = t.ClockOutUtc is null
            ? 0
            : Math.Round((decimal)(t.ClockOutUtc.Value - t.ClockInUtc).TotalHours, 2, MidpointRounding.AwayFromZero),
    };

    private static TimeEntrySignatureDto MapSignature(TimeEntrySignature s) => new()
    {
        SignatureId = s.SignatureId,
        TimeEntryId = s.TimeEntryId,
        Phase = s.Phase,
        SignatureBase64 = s.SignatureBase64,
        SignedByName = s.SignedByName,
        SignedOnUtc = s.SignedOnUtc,
    };

    private static ServiceTypeDto MapService(ServiceType s) => new()
    {
        ServiceTypeId = s.ServiceTypeId,
        Name = s.Name,
        Description = s.Description,
        Category = s.Category,
        ColorTag = s.ColorTag,
        SortOrder = s.SortOrder,
        IsActive = s.IsActive,
        CreatedOn = s.CreatedOn,
        UpdatedOn = s.UpdatedOn,
    };

    private static PortalProfileDto MapProfile(Client c, IReadOnlyList<ServiceType> services) => new()
    {
        ClientId = c.ClientId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        FullName = $"{c.FirstName} {c.LastName}".Trim(),
        Email = c.Email,
        Phone = c.Phone,
        AlternatePhone = c.AlternatePhone,
        AddressLine1 = c.AddressLine1,
        AddressLine2 = c.AddressLine2,
        City = c.City,
        State = c.State,
        PostalCode = c.PostalCode,
        Country = c.Country,
        EmergencyContactName = c.EmergencyContactName,
        EmergencyContactPhone = c.EmergencyContactPhone,
        EmergencyContactRelation = c.EmergencyContactRelation,
        PreferredLanguage = c.PreferredLanguage,
        AccessInstructions = c.AccessInstructions,
        Status = c.Status,
        ServiceTypes = services.Select(MapService).ToList(),
    };
}
