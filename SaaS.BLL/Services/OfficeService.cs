using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Office management with role-based scoping enforced in the service:
/// SuperAdmin = all offices; Admin = only the office they belong to.
/// </summary>
public sealed class OfficeService : IOfficeService
{
    private readonly IOfficeRepository _offices;
    private readonly ITimezoneRepository _timezones;
    private readonly IUserRepository _users;

    public OfficeService(IOfficeRepository offices, ITimezoneRepository timezones, IUserRepository users)
    {
        _offices = offices;
        _timezones = timezones;
        _users = users;
    }

    public async Task<IReadOnlyList<TimezoneDto>> GetTimezonesAsync(CancellationToken ct = default)
    {
        var zones = await _timezones.GetActiveAsync(ct);
        return zones.Select(z => new TimezoneDto
        {
            TimezoneId = z.TimezoneId,
            TimezoneName = z.TimezoneName,
            Description = z.Description,
        }).ToList();
    }

    public async Task<IReadOnlyList<OfficeDto>> GetAllAsync(int currentUserId, int roleId, CancellationToken ct = default)
    {
        var all = await _offices.GetAllAsync(ct);

        // Admins see only their own office; SuperAdmin sees all.
        if (roleId == RoleConstants.AdminId)
        {
            var myOfficeId = await GetCurrentOfficeIdAsync(currentUserId, ct);
            all = all.Where(o => myOfficeId.HasValue && o.OfficeId == myOfficeId.Value).ToList();
        }

        return all.Select(Map).ToList();
    }

    public async Task<OfficeDto?> GetByIdAsync(Guid officeId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanAccessOfficeAsync(officeId, currentUserId, roleId, ct);
        var office = await _offices.GetByIdAsync(officeId, ct);
        return office is null ? null : Map(office);
    }

    public async Task<IReadOnlyList<OfficeMemberDto>> GetMembersAsync(Guid officeId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanAccessOfficeAsync(officeId, currentUserId, roleId, ct);
        var members = await _offices.GetMembersAsync(officeId, ct);
        return members.Select(m => new OfficeMemberDto
        {
            UserId = m.UserId,
            Email = m.Email,
            FullName = m.FullName,
            RoleId = m.RoleId,
            RoleName = m.RoleName ?? string.Empty,
            IsActive = m.IsActive,
        }).ToList();
    }

    public async Task<OfficeDto> CreateAsync(CreateOfficeRequestDto request, CancellationToken ct = default)
    {
        var office = new Office
        {
            OfficeName = request.OfficeName,
            Address = request.Address,
            Phone = request.Phone,
            TimezoneId = request.TimezoneId,
        };
        office.OfficeId = await _offices.CreateAsync(office, ct);

        var created = await _offices.GetByIdAsync(office.OfficeId, ct);
        return created is null ? Map(office) : Map(created);
    }

    public async Task<OfficeDto> UpdateAsync(Guid officeId, UpdateOfficeRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanAccessOfficeAsync(officeId, currentUserId, roleId, ct);

        var existing = await _offices.GetByIdAsync(officeId, ct)
            ?? throw AppException.NotFound("Office not found.");

        existing.OfficeName = request.OfficeName;
        existing.Address = request.Address;
        existing.Phone = request.Phone;
        existing.TimezoneId = request.TimezoneId;
        existing.IsActive = request.IsActive;

        await _offices.UpdateAsync(existing, ct);

        var updated = await _offices.GetByIdAsync(officeId, ct);
        return updated is null ? Map(existing) : Map(updated);
    }

    public Task DeactivateAsync(Guid officeId, CancellationToken ct = default)
        => _offices.DeactivateAsync(officeId, ct);

    // ---- helpers ----

    private async Task<Guid?> GetCurrentOfficeIdAsync(int currentUserId, CancellationToken ct)
    {
        var me = await _users.GetByIdAsync(currentUserId, ct);
        return me?.OfficeId;
    }

    /// <summary>SuperAdmin → any office. Admin → only their own. Others → forbidden.</summary>
    private async Task EnsureCanAccessOfficeAsync(Guid officeId, int currentUserId, int roleId, CancellationToken ct)
    {
        if (roleId == RoleConstants.SuperAdminId)
            return;

        if (roleId == RoleConstants.AdminId)
        {
            var myOfficeId = await GetCurrentOfficeIdAsync(currentUserId, ct);
            if (myOfficeId.HasValue && myOfficeId.Value == officeId)
                return;
        }

        throw AppException.Forbidden("You can only manage your own office.");
    }

    private static OfficeDto Map(Office o) => new()
    {
        OfficeId = o.OfficeId,
        OfficeName = o.OfficeName,
        Address = o.Address,
        Phone = o.Phone,
        TimezoneId = o.TimezoneId,
        TimezoneName = o.TimezoneName,
        IsActive = o.IsActive,
        MemberCount = o.MemberCount,
        CreatedOn = o.CreatedOn,
    };
}
