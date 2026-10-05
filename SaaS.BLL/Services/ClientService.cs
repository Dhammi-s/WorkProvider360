/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.BLL.Common;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Client management. SuperAdmin sees all offices; Admin/Manager are scoped to
/// their own office and their access level (None/Read/Write) comes from
/// ClientSettings. Enabling the portal provisions a Users row (Client role) and
/// emails credentials.
/// </summary>
public sealed class ClientService : IClientService
{
    private const string None = "None";
    private const string Read = "Read";
    private const string Write = "Write";

    private readonly IClientRepository _clients;
    private readonly IClientSettingsRepository _settings;
    private readonly IScheduleRepository _schedules;
    private readonly IUserService _users;
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _email;
    private readonly ITenantContext _tenant;
    private readonly SmtpSettings _smtp;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clients,
        IClientSettingsRepository settings,
        IScheduleRepository schedules,
        IUserService users,
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        IEmailService email,
        ITenantContext tenant,
        IOptions<SmtpSettings> smtp,
        ILogger<ClientService> logger)
    {
        _clients = clients;
        _settings = settings;
        _schedules = schedules;
        _users = users;
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _email = email;
        _tenant = tenant;
        _smtp = smtp.Value;
        _logger = logger;
    }

    // ------------------------------------------------------------ Access / settings

    public async Task<ClientAccessDto> GetAccessAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        return new ClientAccessDto
        {
            RoleName = RoleName(roleId),
            AccessLevel = level,
            IsSuperAdmin = roleId == RoleConstants.SuperAdminId,
            CanViewAll = level is Read or Write,
            CanManage = level == Write,
            CanManageServiceTypes = ServiceTypeLevel(roleId, settings) == Write,
            CanManageSettings = roleId == RoleConstants.SuperAdminId,
        };
    }

    public async Task<ClientSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var s = await _settings.GetAsync(ct);
        return MapSettings(s);
    }

    public async Task<ClientSettingsDto> UpsertSettingsAsync(UpsertClientSettingsDto request, int roleId, CancellationToken ct = default)
    {
        if (roleId != RoleConstants.SuperAdminId)
            throw AppException.Forbidden("Only a SuperAdmin can change client settings.");

        var saved = await _settings.UpsertAsync(new ClientSettings
        {
            AdminClientAccess = CanonLevel(request.AdminClientAccess),
            ManagerClientAccess = CanonLevel(request.ManagerClientAccess),
            AdminServiceTypeAccess = CanonLevel(request.AdminServiceTypeAccess),
            ManagerServiceTypeAccess = CanonLevel(request.ManagerServiceTypeAccess),
            AutoClockInEnabled = request.AutoClockInEnabled,
            AutoClockOutEnabled = request.AutoClockOutEnabled,
            RequireClientSignatureOnClockIn = request.RequireClientSignatureOnClockIn,
            RequireClientSignatureOnClockOut = request.RequireClientSignatureOnClockOut,
            RequireSameOffice = request.RequireSameOffice,
            RequireMatchingSkill = request.RequireMatchingSkill,
            CaptureClockLocation = request.CaptureClockLocation,
            ClientPortalEnabled = request.ClientPortalEnabled,
            SendClientCredentialsEmail = request.SendClientCredentialsEmail,
            NotifyClientOnSchedule = request.NotifyClientOnSchedule,
            RequireClientEmail = request.RequireClientEmail,
            RequireClientPhone = request.RequireClientPhone,
            RequireClientDateOfBirth = request.RequireClientDateOfBirth,
            RequireEmergencyContact = request.RequireEmergencyContact,
            RequireClientServiceTypes = request.RequireClientServiceTypes,
        }, ct);
        return MapSettings(saved);
    }

    // -------------------------------------------------------------------- Clients

    public async Task<PagedResultDto<ClientDto>> GetPagedAsync(
        int page, int pageSize, Guid? officeId, string? status, int? serviceTypeId, string? search,
        int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        if (LevelForRole(roleId, settings) == None)
            throw AppException.Forbidden("You do not have access to clients.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var scopedOffice = await ScopeOfficeAsync(officeId, currentUserId, roleId, ct);

        var (items, total) = await _clients.GetPagedAsync(page, pageSize, scopedOffice, Clean(status), serviceTypeId, Clean(search), ct);
        var list = new List<ClientDto>(items.Count);
        foreach (var c in items)
            list.Add(Map(c, await _clients.GetServiceTypesAsync(c.ClientId, ct)));

        return new PagedResultDto<ClientDto>
        {
            Items = list,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ClientDetailDto?> GetByIdAsync(int clientId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var client = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: false, ct);
        if (client is null) return null;
        var services = await _clients.GetServiceTypesAsync(clientId, ct);
        var dto = new ClientDetailDto();
        CopyInto(dto, client, services);
        return dto;
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        EnsureCanManage(roleId, settings);
        ValidateClient(request, settings);

        var office = await ResolveOfficeForWriteAsync(request.OfficeId, currentUserId, roleId, ct);

        var email = Clean(request.Email);
        if (email is not null && await _clients.EmailExistsAsync(email, null, ct))
            throw AppException.Conflict("A client with this email already exists.");

        var id = await _clients.CreateAsync(BuildEntity(new Client { CreatedByUserId = currentUserId }, request, office), ct);
        await _clients.ReplaceServiceTypesAsync(id, JsonSerializer.Serialize(request.ServiceTypeIds ?? new List<int>()), ct);

        return await BuildDtoAsync(id, ct);
    }

    public async Task<ClientDto> UpdateAsync(int clientId, UpdateClientRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        _ = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: true, ct)
            ?? throw AppException.NotFound("Client not found.");
        ValidateClient(request, settings);

        var office = await ResolveOfficeForWriteAsync(request.OfficeId, currentUserId, roleId, ct);

        var email = Clean(request.Email);
        if (email is not null && await _clients.EmailExistsAsync(email, clientId, ct))
            throw AppException.Conflict("A client with this email already exists.");

        var entity = BuildEntity(new Client { ClientId = clientId }, request, office);
        await _clients.UpdateAsync(entity, ct);
        await _clients.ReplaceServiceTypesAsync(clientId, JsonSerializer.Serialize(request.ServiceTypeIds ?? new List<int>()), ct);

        return await BuildDtoAsync(clientId, ct);
    }

    public async Task UpdateStatusAsync(int clientId, UpdateClientStatusRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        _ = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: true, ct)
            ?? throw AppException.NotFound("Client not found.");

        var status = (request.Status ?? string.Empty).Trim();
        if (status is not ("Active" or "OnHold" or "Inactive"))
            throw AppException.BadRequest("Status must be Active, OnHold or Inactive.");

        await _clients.UpdateStatusAsync(clientId, status, ct);
    }

    // --------------------------------------------------------------- Portal access

    public async Task<ClientDto> SetPortalAccessAsync(int clientId, SetClientPortalAccessRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        var client = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: true, ct)
            ?? throw AppException.NotFound("Client not found.");

        if (request.Enabled)
        {
            if (settings?.ClientPortalEnabled == false)
                throw AppException.BadRequest("The client portal is disabled for this agency.");
            if (string.IsNullOrWhiteSpace(client.Email))
                throw AppException.BadRequest("The client needs an email address before portal access can be enabled.");

            var sendEmail = request.SendEmail ?? settings?.SendClientCredentialsEmail ?? true;
            var tempPassword = GenerateTemporaryPassword();
            var displayName = $"{client.FirstName} {client.LastName}".Trim();

            if (client.UserId is null)
            {
                var user = await _users.CreateAsync(new CreateUserRequestDto
                {
                    Email = client.Email!,
                    FullName = displayName,
                    Password = tempPassword,
                    RoleId = RoleConstants.ClientId,
                    OfficeId = client.OfficeId,
                }, ct);
                await _clients.SetPortalUserAsync(clientId, user.UserId, true, ct);
            }
            else
            {
                var (hash, salt) = _passwordHasher.HashPassword(tempPassword);
                await _userRepo.UpdatePasswordAsync(client.UserId.Value, hash, salt, ct);
                await _clients.SetPortalUserAsync(clientId, client.UserId, true, ct);
            }

            if (sendEmail)
                await TrySendWelcomeAsync(client.Email!, displayName, tempPassword, clientId, ct);
        }
        else
        {
            await _clients.SetPortalUserAsync(clientId, client.UserId, false, ct);
        }

        return await BuildDtoAsync(clientId, ct);
    }

    public async Task ResendCredentialsAsync(int clientId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        var client = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: true, ct)
            ?? throw AppException.NotFound("Client not found.");

        if (client.UserId is null || string.IsNullOrWhiteSpace(client.Email))
            throw AppException.BadRequest("This client has no portal login to send credentials to.");

        var tempPassword = GenerateTemporaryPassword();
        var displayName = $"{client.FirstName} {client.LastName}".Trim();

        try
        {
            await _email.SendClientWelcomeAsync(client.Email!, displayName, client.Email!, tempPassword, BuildLoginUrl(), ct);
        }
        catch (Exception ex)
        {
            throw new AppException($"Could not send the email, so the password was left unchanged. {ex.Message}", 502);
        }

        var (h, s) = _passwordHasher.HashPassword(tempPassword);
        await _userRepo.UpdatePasswordAsync(client.UserId.Value, h, s, ct);
    }

    // ------------------------------------------------- Eligible caregivers / visits

    public async Task<IReadOnlyList<EligibleCaregiverDto>> GetEligibleCaregiversAsync(int clientId, int? serviceTypeId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        _ = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: true, ct)
            ?? throw AppException.NotFound("Client not found.");

        var rows = await _clients.GetEligibleCaregiversAsync(clientId, serviceTypeId, ct);
        return rows.Select(r => new EligibleCaregiverDto
        {
            UserId = r.UserId,
            FullName = r.FullName,
            AvatarUrl = r.AvatarUrl,
            OfficeId = r.OfficeId,
            OfficeName = r.OfficeName,
            IsSameOffice = r.IsSameOffice,
            HasSkill = r.HasSkill,
            Skills = SplitNames(r.SkillNames),
        }).ToList();
    }

    public async Task<IReadOnlyList<ClientVisitDto>> GetVisitsAsync(int clientId, DateTime? fromUtc, DateTime? toUtc, int currentUserId, int roleId, CancellationToken ct = default)
    {
        _ = await LoadForCallerAsync(clientId, currentUserId, roleId, requireWrite: false, ct)
            ?? throw AppException.NotFound("Client not found.");

        var rows = await _schedules.GetByClientAsync(clientId, fromUtc, toUtc, null, ct);
        return rows.Select(MapVisit).ToList();
    }

    // ------------------------------------------------------------------- Helpers

    private async Task TrySendWelcomeAsync(string email, string name, string tempPassword, int clientId, CancellationToken ct)
    {
        try
        {
            await _email.SendClientWelcomeAsync(email, name, email, tempPassword, BuildLoginUrl(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enabled portal for client {ClientId} but failed to send the welcome email.", clientId);
        }
    }

    private async Task<Client?> LoadForCallerAsync(int clientId, int currentUserId, int roleId, bool requireWrite, CancellationToken ct)
    {
        var settings = await _settings.GetAsync(ct);
        var level = LevelForRole(roleId, settings);
        if (level == None) throw AppException.Forbidden("You do not have access to clients.");
        if (requireWrite && level != Write) throw AppException.Forbidden("You do not have permission to manage clients.");

        var client = await _clients.GetByIdAsync(clientId, ct);
        if (client is null) return null;

        if (roleId is RoleConstants.AdminId or RoleConstants.ManagerId)
        {
            var myOffice = (await _users.GetByIdAsync(currentUserId, ct))?.OfficeId;
            if (client.OfficeId != myOffice)
                throw AppException.Forbidden("This client belongs to another office.");
        }
        return client;
    }

    private async Task<Guid?> ScopeOfficeAsync(Guid? requestedOffice, int currentUserId, int roleId, CancellationToken ct)
    {
        if (roleId is RoleConstants.AdminId or RoleConstants.ManagerId)
            return (await _users.GetByIdAsync(currentUserId, ct))?.OfficeId;
        return requestedOffice;
    }

    private async Task<Guid?> ResolveOfficeForWriteAsync(Guid? requestedOffice, int currentUserId, int roleId, CancellationToken ct)
    {
        if (roleId is RoleConstants.AdminId or RoleConstants.ManagerId)
            return (await _users.GetByIdAsync(currentUserId, ct))?.OfficeId;
        return requestedOffice;
    }

    private async Task<ClientDto> BuildDtoAsync(int clientId, CancellationToken ct)
    {
        var client = await _clients.GetByIdAsync(clientId, ct)
            ?? throw AppException.NotFound("Client not found.");
        var services = await _clients.GetServiceTypesAsync(clientId, ct);
        return Map(client, services);
    }

    private static void ValidateClient(CreateClientRequestDto r, ClientSettings? s)
    {
        if ((s?.RequireClientEmail ?? true) && string.IsNullOrWhiteSpace(r.Email))
            throw AppException.BadRequest("Email is required.");
        if ((s?.RequireClientPhone ?? true) && string.IsNullOrWhiteSpace(r.Phone))
            throw AppException.BadRequest("Phone is required.");
        if ((s?.RequireClientDateOfBirth ?? false) && r.DateOfBirth is null)
            throw AppException.BadRequest("Date of birth is required.");
        if ((s?.RequireEmergencyContact ?? false) &&
            (string.IsNullOrWhiteSpace(r.EmergencyContactName) || string.IsNullOrWhiteSpace(r.EmergencyContactPhone)))
            throw AppException.BadRequest("An emergency contact name and phone are required.");
        if ((s?.RequireClientServiceTypes ?? true) && (r.ServiceTypeIds is null || r.ServiceTypeIds.Count == 0))
            throw AppException.BadRequest("Please select at least one service the client needs.");
    }

    private static Client BuildEntity(Client target, CreateClientRequestDto r, Guid? office)
    {
        target.OfficeId = office;
        target.FirstName = r.FirstName.Trim();
        target.LastName = r.LastName.Trim();
        target.Email = Clean(r.Email);
        target.Phone = Clean(r.Phone);
        target.AlternatePhone = Clean(r.AlternatePhone);
        target.DateOfBirth = r.DateOfBirth;
        target.Gender = Clean(r.Gender);
        target.AddressLine1 = r.AddressLine1.Trim();
        target.AddressLine2 = Clean(r.AddressLine2);
        target.City = Clean(r.City);
        target.State = Clean(r.State);
        target.PostalCode = Clean(r.PostalCode);
        target.Country = Clean(r.Country);
        target.Latitude = r.Latitude;
        target.Longitude = r.Longitude;
        target.EmergencyContactName = Clean(r.EmergencyContactName);
        target.EmergencyContactPhone = Clean(r.EmergencyContactPhone);
        target.EmergencyContactRelation = Clean(r.EmergencyContactRelation);
        target.PreferredLanguage = Clean(r.PreferredLanguage);
        target.AccessInstructions = Clean(r.AccessInstructions);
        target.CareNotes = Clean(r.CareNotes);
        target.Allergies = Clean(r.Allergies);
        target.MobilityNotes = Clean(r.MobilityNotes);
        target.Status = "Active";
        target.StartDate = r.StartDate;
        target.Notes = Clean(r.Notes);
        return target;
    }

    private string BuildLoginUrl()
    {
        var baseUrl = FrontendUrls.ResolveOrigin(_tenant.Agency?.DomainUrl, _smtp.ResetPasswordBaseUrl);
        return string.IsNullOrEmpty(baseUrl) ? "/login" : $"{baseUrl}/login";
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var core = new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        return core + "Aa9!";
    }

    private static IReadOnlyList<string> SplitNames(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? new List<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static string RoleName(int roleId) => roleId switch
    {
        RoleConstants.SuperAdminId => RoleConstants.SuperAdmin,
        RoleConstants.AdminId => RoleConstants.Admin,
        RoleConstants.ManagerId => RoleConstants.Manager,
        RoleConstants.UserId => RoleConstants.User,
        RoleConstants.ClientId => RoleConstants.Client,
        _ => string.Empty,
    };

    private static string LevelForRole(int roleId, ClientSettings? s)
    {
        if (roleId == RoleConstants.SuperAdminId) return Write;
        if (roleId == RoleConstants.AdminId) return CanonLevel(s?.AdminClientAccess ?? Write);
        if (roleId == RoleConstants.ManagerId) return CanonLevel(s?.ManagerClientAccess ?? Read);
        return None;
    }

    private static string ServiceTypeLevel(int roleId, ClientSettings? s)
    {
        if (roleId == RoleConstants.SuperAdminId) return Write;
        if (roleId == RoleConstants.AdminId) return CanonLevel(s?.AdminServiceTypeAccess ?? Write);
        if (roleId == RoleConstants.ManagerId) return CanonLevel(s?.ManagerServiceTypeAccess ?? Write);
        return None;
    }

    private static void EnsureCanManage(int roleId, ClientSettings? settings)
    {
        if (LevelForRole(roleId, settings) != Write)
            throw AppException.Forbidden("You do not have permission to manage clients.");
    }

    private static string CanonLevel(string? value)
    {
        if (string.Equals(value, Write, StringComparison.OrdinalIgnoreCase)) return Write;
        if (string.Equals(value, Read, StringComparison.OrdinalIgnoreCase)) return Read;
        return None;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ClientVisitDto MapVisit(ClientVisit v) => new()
    {
        ScheduleId = v.ScheduleId,
        Title = v.Title,
        ClientId = v.ClientId,
        ServiceTypeId = v.ServiceTypeId,
        ServiceTypeName = v.ServiceTypeName,
        AssignedUserId = v.AssignedUserId,
        AssignedUserName = v.AssignedUserName,
        AssignedUserAvatarUrl = v.AssignedUserAvatarUrl,
        Location = v.Location,
        StartUtc = v.StartUtc,
        EndUtc = v.EndUtc,
        Status = v.Status,
        ClockInUtc = v.ClockInUtc,
        ClockOutUtc = v.ClockOutUtc,
        WorkedSeconds = v.WorkedSeconds,
        HasClockOutSignature = v.HasClockOutSignature,
    };

    private static ClientSettingsDto MapSettings(ClientSettings? s) => new()
    {
        AdminClientAccess = CanonLevel(s?.AdminClientAccess ?? Write),
        ManagerClientAccess = CanonLevel(s?.ManagerClientAccess ?? Read),
        AdminServiceTypeAccess = CanonLevel(s?.AdminServiceTypeAccess ?? Write),
        ManagerServiceTypeAccess = CanonLevel(s?.ManagerServiceTypeAccess ?? Write),
        AutoClockInEnabled = s?.AutoClockInEnabled ?? false,
        AutoClockOutEnabled = s?.AutoClockOutEnabled ?? false,
        RequireClientSignatureOnClockIn = s?.RequireClientSignatureOnClockIn ?? false,
        RequireClientSignatureOnClockOut = s?.RequireClientSignatureOnClockOut ?? true,
        RequireSameOffice = s?.RequireSameOffice ?? true,
        RequireMatchingSkill = s?.RequireMatchingSkill ?? false,
        CaptureClockLocation = s?.CaptureClockLocation ?? true,
        ClientPortalEnabled = s?.ClientPortalEnabled ?? true,
        SendClientCredentialsEmail = s?.SendClientCredentialsEmail ?? true,
        NotifyClientOnSchedule = s?.NotifyClientOnSchedule ?? true,
        RequireClientEmail = s?.RequireClientEmail ?? true,
        RequireClientPhone = s?.RequireClientPhone ?? true,
        RequireClientDateOfBirth = s?.RequireClientDateOfBirth ?? false,
        RequireEmergencyContact = s?.RequireEmergencyContact ?? false,
        RequireClientServiceTypes = s?.RequireClientServiceTypes ?? true,
        UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
    };

    private static ClientDto Map(Client c, IReadOnlyList<ServiceType> services)
    {
        var dto = new ClientDto();
        CopyInto(dto, c, services);
        return dto;
    }

    private static void CopyInto(ClientDto dto, Client c, IReadOnlyList<ServiceType> services)
    {
        dto.ClientId = c.ClientId;
        dto.UserId = c.UserId;
        dto.OfficeId = c.OfficeId;
        dto.OfficeName = c.OfficeName;
        dto.FirstName = c.FirstName;
        dto.LastName = c.LastName;
        dto.FullName = $"{c.FirstName} {c.LastName}".Trim();
        dto.Email = c.Email;
        dto.Phone = c.Phone;
        dto.AlternatePhone = c.AlternatePhone;
        dto.DateOfBirth = c.DateOfBirth;
        dto.Gender = c.Gender;
        dto.AddressLine1 = c.AddressLine1;
        dto.AddressLine2 = c.AddressLine2;
        dto.City = c.City;
        dto.State = c.State;
        dto.PostalCode = c.PostalCode;
        dto.Country = c.Country;
        dto.Latitude = c.Latitude;
        dto.Longitude = c.Longitude;
        dto.EmergencyContactName = c.EmergencyContactName;
        dto.EmergencyContactPhone = c.EmergencyContactPhone;
        dto.EmergencyContactRelation = c.EmergencyContactRelation;
        dto.PreferredLanguage = c.PreferredLanguage;
        dto.AccessInstructions = c.AccessInstructions;
        dto.CareNotes = c.CareNotes;
        dto.Allergies = c.Allergies;
        dto.MobilityNotes = c.MobilityNotes;
        dto.PortalEnabled = c.PortalEnabled;
        dto.Status = c.Status;
        dto.StartDate = c.StartDate;
        dto.Notes = c.Notes;
        dto.CreatedOn = c.CreatedOn;
        dto.UpdatedOn = c.UpdatedOn;
        dto.PortalUserId = c.UserId;
        dto.PortalEmail = c.PortalEmail;
        dto.PortalIsLockedOut = c.PortalIsLockedOut ?? false;
        dto.ServiceTypes = services.Select(s => new ServiceTypeDto
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
        }).ToList();
    }
}
