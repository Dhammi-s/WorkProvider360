/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Service-type (skills/services) management. SuperAdmin always has full write;
/// Admin/Manager get None/Read/Write from ClientSettings. Deactivate is a soft
/// delete so historical schedules keep their service name.
/// </summary>
public sealed class ServiceTypeService : IServiceTypeService
{
    private const string None = "None";
    private const string Read = "Read";
    private const string Write = "Write";

    private readonly IServiceTypeRepository _serviceTypes;
    private readonly IClientSettingsRepository _settings;

    public ServiceTypeService(IServiceTypeRepository serviceTypes, IClientSettingsRepository settings)
    {
        _serviceTypes = serviceTypes;
        _settings = settings;
    }

    public async Task<IReadOnlyList<ServiceTypeDto>> GetAllAsync(int roleId, CancellationToken ct = default)
    {
        var settings = await _settings.GetAsync(ct);
        if (LevelForRole(roleId, settings) == None)
            throw AppException.Forbidden("You do not have access to service types.");
        var rows = await _serviceTypes.GetAllAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ServiceTypeDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var rows = await _serviceTypes.GetActiveAsync(ct);
        return rows.Select(Map).ToList();
    }

    public async Task<ServiceTypeDto?> GetByIdAsync(int serviceTypeId, CancellationToken ct = default)
    {
        var row = await _serviceTypes.GetByIdAsync(serviceTypeId, ct);
        return row is null ? null : Map(row);
    }

    public async Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(roleId, ct);

        var name = request.Name.Trim();
        if (await _serviceTypes.NameExistsAsync(name, null, ct))
            throw AppException.Conflict("A service type with this name already exists.");

        var id = await _serviceTypes.CreateAsync(new ServiceType
        {
            Name = name,
            Description = Clean(request.Description),
            Category = Clean(request.Category),
            ColorTag = Clean(request.ColorTag),
            SortOrder = request.SortOrder,
        }, currentUserId, ct);

        var created = await _serviceTypes.GetByIdAsync(id, ct);
        return created is null ? throw AppException.NotFound("Service type not found after creation.") : Map(created);
    }

    public async Task<ServiceTypeDto> UpdateAsync(int serviceTypeId, UpdateServiceTypeRequestDto request, int roleId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(roleId, ct);

        var existing = await _serviceTypes.GetByIdAsync(serviceTypeId, ct)
            ?? throw AppException.NotFound("Service type not found.");

        var name = request.Name.Trim();
        if (await _serviceTypes.NameExistsAsync(name, serviceTypeId, ct))
            throw AppException.Conflict("A service type with this name already exists.");

        existing.Name = name;
        existing.Description = Clean(request.Description);
        existing.Category = Clean(request.Category);
        existing.ColorTag = Clean(request.ColorTag);
        existing.SortOrder = request.SortOrder;
        existing.IsActive = request.IsActive;
        await _serviceTypes.UpdateAsync(existing, ct);

        var updated = await _serviceTypes.GetByIdAsync(serviceTypeId, ct);
        return updated is null ? Map(existing) : Map(updated);
    }

    public async Task DeactivateAsync(int serviceTypeId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(roleId, ct);
        _ = await _serviceTypes.GetByIdAsync(serviceTypeId, ct)
            ?? throw AppException.NotFound("Service type not found.");
        await _serviceTypes.DeactivateAsync(serviceTypeId, ct);
    }

    private async Task EnsureCanManageAsync(int roleId, CancellationToken ct)
    {
        var settings = await _settings.GetAsync(ct);
        if (LevelForRole(roleId, settings) != Write)
            throw AppException.Forbidden("You do not have permission to manage service types.");
    }

    private static string LevelForRole(int roleId, ClientSettings? s)
    {
        if (roleId == RoleConstants.SuperAdminId) return Write;
        if (roleId == RoleConstants.AdminId) return CanonLevel(s?.AdminServiceTypeAccess ?? Write);
        if (roleId == RoleConstants.ManagerId) return CanonLevel(s?.ManagerServiceTypeAccess ?? Write);
        return None;
    }

    private static string CanonLevel(string? value)
    {
        if (string.Equals(value, Write, StringComparison.OrdinalIgnoreCase)) return Write;
        if (string.Equals(value, Read, StringComparison.OrdinalIgnoreCase)) return Read;
        return None;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ServiceTypeDto Map(ServiceType s) => new()
    {
        ServiceTypeId = s.ServiceTypeId,
        Name = s.Name,
        Description = s.Description,
        Category = s.Category,
        ColorTag = s.ColorTag,
        SortOrder = s.SortOrder,
        IsActive = s.IsActive,
        ClientCount = s.ClientCount,
        UserCount = s.UserCount,
        CreatedOn = s.CreatedOn,
        UpdatedOn = s.UpdatedOn,
    };
}
