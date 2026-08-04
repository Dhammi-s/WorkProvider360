/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;
using SaaS.BLL.Common;

namespace SaaS.BLL.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _email;
    private readonly ICloudinaryService _cloudinary;
    private readonly IApplicationSettingsRepository _appSettings;
    private readonly ITenantContext _tenant;
    private readonly SmtpSettings _smtp;

    public UserService(
        IUserRepository users,
        IRoleRepository roles,
        IPasswordHasher passwordHasher,
        IEmailService email,
        ICloudinaryService cloudinary,
        IApplicationSettingsRepository appSettings,
        ITenantContext tenant,
        IOptions<SmtpSettings> smtp)
    {
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _email = email;
        _cloudinary = cloudinary;
        _appSettings = appSettings;
        _tenant = tenant;
        _smtp = smtp.Value;
    }

    /// <summary>
    /// Whether the current tenant lets Admins and Managers (not only SuperAdmin)
    /// unlock locked accounts. Surfaced to the Team page to decide button visibility.
    /// </summary>
    public async Task<bool> GetAllowStaffUnlockAsync(CancellationToken ct = default)
    {
        var settings = await _appSettings.GetAsync(ct);
        return settings?.AllowStaffUnlock ?? false;
    }

    /// <summary>
    /// Unlocks a locked account: resets the password to a fresh temporary one and
    /// emails the user (naming who unlocked them) before clearing the lock.
    /// SuperAdmin may unlock anyone. Admin / Manager may unlock only when the tenant
    /// flag allows it AND the target is strictly lower in rank than the actor.
    /// </summary>
    public async Task UnlockAsync(int actingUserId, int actingRoleId, int targetUserId, CancellationToken ct = default)
    {
        var target = await _users.GetByIdAsync(targetUserId, ct)
            ?? throw AppException.NotFound("User not found.");

        if (actingRoleId != RoleConstants.SuperAdminId)
        {
            // Admin / Manager gate: tenant flag must be on...
            if (!await GetAllowStaffUnlockAsync(ct))
                throw AppException.Forbidden("Unlocking accounts is restricted to SuperAdmins for this agency.");

            // ...and they can only unlock someone below their own rank.
            // Lower RoleId = higher rank, so the target's id must be greater.
            if (target.RoleId <= actingRoleId)
                throw AppException.Forbidden("You can only unlock accounts below your own role.");
        }

        // Who is unlocking — shown in the email, e.g. "Alex Morgan (Admin)".
        var actor = await _users.GetByIdAsync(actingUserId, ct);
        var unlockedBy = actor is null
            ? "an administrator"
            : $"{actor.FullName} ({actor.RoleName})";

        var newPassword = GenerateTemporaryPassword();

        // Email the new password BEFORE changing anything: if delivery fails we
        // leave the account locked and the old password intact, and surface why.
        try
        {
            await _email.SendAccountUnlockedAsync(target.Email, target.FullName, target.Email, unlockedBy, newPassword, BuildLoginUrl(), ct);
        }
        catch (Exception ex)
        {
            throw new AppException(
                $"Could not send the unlock email, so the account was left locked. {ex.Message}", 502);
        }

        var (hash, salt) = _passwordHasher.HashPassword(newPassword);
        await _users.UpdatePasswordAsync(targetUserId, hash, salt, ct);
        await _users.SetLockoutAsync(targetUserId, false, ct);
    }

    /// <summary>Uploads a cropped avatar to Cloudinary and stores its URL on the user.</summary>
    public async Task<UserDto> UpdateAvatarAsync(int userId, string imageBase64, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageBase64) || !imageBase64.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("Please provide a valid image.");

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw AppException.NotFound("User not found.");

        var url = await _cloudinary.UploadImageAsync(imageBase64, "workprovider360/avatars", ct);
        await _users.UpdateAvatarAsync(userId, url, ct);

        user.AvatarUrl = url;
        return Map(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _users.GetAllAsync(ct);
        return users.Select(Map).ToList();
    }

    public async Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? roleName, Guid? officeId, bool noOffice, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await _users.GetPagedAsync(page, pageSize, roleName, officeId, noOffice, ct);
        return new PagedResultDto<UserDto>
        {
            Items = items.Select(Map).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
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
        // Prefer the resolved agency's own domain so each tenant's emails link to
        // their own front-end; fall back to the configured base URL.
        var baseUrl = FrontendUrls.ResolveOrigin(_tenant.Agency?.DomainUrl, _smtp.ResetPasswordBaseUrl);
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
        AvatarUrl = u.AvatarUrl,
        OfficeId = u.OfficeId,
        OfficeName = u.OfficeName,
        Salary = u.Salary,
        IsActive = u.IsActive,
        IsLockedOut = u.IsLockedOut,
        CreatedOn = u.CreatedOn,
    };
}
