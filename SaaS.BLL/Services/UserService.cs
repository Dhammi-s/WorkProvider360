using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users, IRoleRepository roles, IPasswordHasher passwordHasher)
    {
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _users.GetAllAsync(ct);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequestDto request, CancellationToken ct = default)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, ct)
            ?? throw AppException.BadRequest("Specified role does not exist.");

        if (await _users.EmailExistsAsync(request.Email, ct))
            throw AppException.Conflict("A user with this email already exists.");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new AppUser
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = request.RoleId,
            IsActive = true,
        };

        user.UserId = await _users.CreateAsync(user, ct);
        user.RoleName = role.RoleName;
        return Map(user);
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserRequestDto request, CancellationToken ct = default)
    {
        if (await _users.EmailExistsAsync(request.Email, ct))
            throw AppException.Conflict("A user with this email already exists.");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new AppUser
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = RoleConstants.UserId,
            IsActive = true,
        };

        user.UserId = await _users.CreateAsync(user, ct);
        user.RoleName = RoleConstants.User;
        return Map(user);
    }

    public async Task<UserDto> BootstrapFirstAdminAsync(BootstrapAdminRequestDto request, CancellationToken ct = default)
    {
        var existing = await _users.GetAllAsync(ct);
        if (existing.Count > 0)
            throw AppException.Conflict("This tenant already has users; bootstrap is disabled.");

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var user = new AppUser
        {
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = RoleConstants.SuperAdminId,
            IsActive = true,
        };

        user.UserId = await _users.CreateAsync(user, ct);
        user.RoleName = RoleConstants.SuperAdmin;
        return Map(user);
    }

    private static UserDto Map(AppUser u) => new()
    {
        UserId = u.UserId,
        Email = u.Email,
        FullName = u.FullName,
        RoleId = u.RoleId,
        RoleName = u.RoleName ?? string.Empty,
        IsActive = u.IsActive,
        CreatedOn = u.CreatedOn,
    };
}
