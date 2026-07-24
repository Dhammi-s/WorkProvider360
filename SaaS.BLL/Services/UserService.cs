using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _email;
    private readonly SmtpSettings _smtp;

    public UserService(
        IUserRepository users,
        IRoleRepository roles,
        IPasswordHasher passwordHasher,
        IEmailService email,
        IOptions<SmtpSettings> smtp)
    {
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _email = email;
        _smtp = smtp.Value;
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
            OfficeId = request.OfficeId,
            Salary = request.Salary,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
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

    public async Task ResendCredentialsAsync(int userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw AppException.NotFound("User not found.");

        var newPassword = GenerateTemporaryPassword();

        // Send the email BEFORE changing the password: if the email can't be sent
        // we leave the existing password intact (so the user isn't locked out) and
        // surface the real reason instead of a generic 500.
        try
        {
            await _email.SendCredentialsAsync(user.Email, user.FullName, user.Email, newPassword, BuildLoginUrl(), ct);
        }
        catch (Exception ex)
        {
            throw new AppException(
                $"Could not send the email, so the password was left unchanged. {ex.Message}", 502);
        }

        var (hash, salt) = _passwordHasher.HashPassword(newPassword);
        await _users.UpdatePasswordAsync(userId, hash, salt, ct);
    }

    public async Task<BulkOperationResultDto> ResendCredentialsBulkAsync(IReadOnlyList<int> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        var result = new BulkOperationResultDto { Total = ids.Count };
        foreach (var id in ids)
        {
            try
            {
                await ResendCredentialsAsync(id, ct);
                result.Succeeded++;
            }
            catch
            {
                result.Failed++;
            }
        }
        return result;
    }

    private string BuildLoginUrl()
    {
        var baseUrl = (_smtp.ResetPasswordBaseUrl ?? string.Empty).Trim();
        const string resetSuffix = "/reset-password";
        if (baseUrl.EndsWith(resetSuffix, StringComparison.OrdinalIgnoreCase))
            baseUrl = baseUrl[..^resetSuffix.Length];
        baseUrl = baseUrl.TrimEnd('/');
        return string.IsNullOrEmpty(baseUrl) ? "/login" : $"{baseUrl}/login";
    }

    /// <summary>A random 14-char password satisfying the 8+ minimum with mixed classes.</summary>
    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var core = new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        return core + "Aa9!";
    }

    private static UserDto Map(AppUser u) => new()
    {
        UserId = u.UserId,
        Email = u.Email,
        FullName = u.FullName,
        RoleId = u.RoleId,
        RoleName = u.RoleName ?? string.Empty,
        Phone = u.Phone,
        OfficeId = u.OfficeId,
        OfficeName = u.OfficeName,
        Salary = u.Salary,
        IsActive = u.IsActive,
        CreatedOn = u.CreatedOn,
    };
}
