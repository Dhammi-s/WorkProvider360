using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _roles;

    public RoleService(IRoleRepository roles) => _roles = roles;

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await _roles.GetAllAsync(ct);
        return roles.Select(Map).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(int roleId, CancellationToken ct = default)
    {
        var role = await _roles.GetByIdAsync(roleId, ct);
        return role is null ? null : Map(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequestDto request, CancellationToken ct = default)
    {
        if (await _roles.GetByIdAsync(request.RoleId, ct) is not null)
            throw AppException.Conflict($"A role with id {request.RoleId} already exists.");

        if (await _roles.RoleNameExistsAsync(request.RoleName, ct))
            throw AppException.Conflict($"A role named '{request.RoleName}' already exists.");

        var role = new Role
        {
            RoleId = request.RoleId,
            RoleName = request.RoleName,
            IsActive = request.IsActive,
        };

        await _roles.CreateAsync(role, ct);
        return Map(role);
    }

    private static RoleDto Map(Role r) => new()
    {
        RoleId = r.RoleId,
        RoleName = r.RoleName,
        IsActive = r.IsActive,
    };
}
