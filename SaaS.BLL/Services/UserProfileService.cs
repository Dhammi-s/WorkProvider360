/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Text.Json;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Staff profile, skills and availability. A user may always manage their own
/// profile; SuperAdmin/Admin/Manager may manage staff who rank strictly below them.
/// </summary>
public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _profiles;
    private readonly IUserService _users;

    public UserProfileService(IUserProfileRepository profiles, IUserService users)
    {
        _profiles = profiles;
        _users = users;
    }

    public async Task<UserProfileDto> GetAsync(int targetUserId, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanAccessAsync(targetUserId, currentUserId, roleId, ct);
        return await BuildAsync(targetUserId, ct);
    }

    public async Task<UserProfileDto> UpsertAsync(int targetUserId, UpsertUserProfileRequestDto request, int currentUserId, int roleId, CancellationToken ct = default)
    {
        await EnsureCanAccessAsync(targetUserId, currentUserId, roleId, ct);

        await _profiles.UpsertAsync(new UserProfile
        {
            UserId = targetUserId,
            AddressLine1 = Clean(request.AddressLine1),
            AddressLine2 = Clean(request.AddressLine2),
            City = Clean(request.City),
            State = Clean(request.State),
            PostalCode = Clean(request.PostalCode),
            Country = Clean(request.Country),
            DateOfBirth = request.DateOfBirth,
            Gender = Clean(request.Gender),
            Qualifications = Clean(request.Qualifications),
            YearsOfExperience = request.YearsOfExperience,
            About = Clean(request.About),
            HasDrivingLicense = request.HasDrivingLicense,
            HasVehicle = request.HasVehicle,
            EmergencyContactName = Clean(request.EmergencyContactName),
            EmergencyContactPhone = Clean(request.EmergencyContactPhone),
            HireDate = request.HireDate,
        }, ct);

        await _profiles.ReplaceServiceTypesAsync(targetUserId, JsonSerializer.Serialize(request.ServiceTypeIds ?? new List<int>()), ct);
        await _profiles.ReplaceAvailabilityAsync(targetUserId, SerializeAvailability(request.Availability), ct);

        return await BuildAsync(targetUserId, ct);
    }

    private async Task<UserProfileDto> BuildAsync(int userId, CancellationToken ct)
    {
        var profile = await _profiles.GetAsync(userId, ct);
        var skills = await _profiles.GetServiceTypesAsync(userId, ct);
        var availability = await _profiles.GetAvailabilityAsync(userId, ct);

        return new UserProfileDto
        {
            UserId = userId,
            AddressLine1 = profile?.AddressLine1,
            AddressLine2 = profile?.AddressLine2,
            City = profile?.City,
            State = profile?.State,
            PostalCode = profile?.PostalCode,
            Country = profile?.Country,
            DateOfBirth = profile?.DateOfBirth,
            Gender = profile?.Gender,
            Qualifications = profile?.Qualifications,
            YearsOfExperience = profile?.YearsOfExperience,
            About = profile?.About,
            HasDrivingLicense = profile?.HasDrivingLicense ?? false,
            HasVehicle = profile?.HasVehicle ?? false,
            EmergencyContactName = profile?.EmergencyContactName,
            EmergencyContactPhone = profile?.EmergencyContactPhone,
            HireDate = profile?.HireDate,
            Skills = skills.Select(MapService).ToList(),
            Availability = availability.Select(MapSlot).ToList(),
        };
    }

    private async Task EnsureCanAccessAsync(int targetUserId, int currentUserId, int roleId, CancellationToken ct)
    {
        if (targetUserId == currentUserId) return; // own profile
        if (roleId == RoleConstants.SuperAdminId) return;
        if (roleId is RoleConstants.AdminId or RoleConstants.ManagerId)
        {
            var target = await _users.GetByIdAsync(targetUserId, ct)
                ?? throw AppException.NotFound("User not found.");
            // Lower RoleId = higher rank; may only manage staff strictly below.
            if (target.RoleId > roleId) return;
        }
        throw AppException.Forbidden("You cannot manage this user's profile.");
    }

    /// <summary>Serializes availability with the exact JSON keys the OPENJSON proc expects.</summary>
    private static string SerializeAvailability(IEnumerable<AvailabilitySlotDto>? slots)
    {
        var list = (slots ?? Enumerable.Empty<AvailabilitySlotDto>())
            .Where(s => !string.IsNullOrWhiteSpace(s.StartTime) && !string.IsNullOrWhiteSpace(s.EndTime))
            .Select(s => new { s.DayOfWeek, s.StartTime, s.EndTime });
        return JsonSerializer.Serialize(list);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

    private static AvailabilitySlotDto MapSlot(AvailabilitySlot a) => new()
    {
        DayOfWeek = a.DayOfWeek,
        StartTime = a.StartTime.ToString(@"hh\:mm"),
        EndTime = a.EndTime.ToString(@"hh\:mm"),
    };
}
