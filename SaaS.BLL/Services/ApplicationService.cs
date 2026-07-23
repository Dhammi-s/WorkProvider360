using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
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

/// <summary>
/// Role-application workflow. Public submission + config, SuperAdmin/Admin review
/// (approve creates the user + emails credentials; reject emails a notice), and
/// SuperAdmin-managed form settings and custom questions.
/// </summary>
public sealed class ApplicationService : IApplicationService
{
    /// <summary>Roles a person may request through the public form.</summary>
    private static readonly int[] ApplyableRoleIds = { RoleConstants.AdminId, RoleConstants.ManagerId };

    private readonly IApplicationRepository _applications;
    private readonly IApplicationQuestionRepository _questions;
    private readonly IApplicationSettingsRepository _settings;
    private readonly IRoleService _roles;
    private readonly IUserService _users;
    private readonly IEmailService _email;
    private readonly SmtpSettings _smtp;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository applications,
        IApplicationQuestionRepository questions,
        IApplicationSettingsRepository settings,
        IRoleService roles,
        IUserService users,
        IEmailService email,
        IOptions<SmtpSettings> smtp,
        ILogger<ApplicationService> logger)
    {
        _applications = applications;
        _questions = questions;
        _settings = settings;
        _roles = roles;
        _users = users;
        _email = email;
        _smtp = smtp.Value;
        _logger = logger;
    }

    // ----------------------------------------------------------------- Public

    public async Task<PublicFormConfigDto> GetPublicFormConfigAsync(CancellationToken ct = default)
    {
        var roles = await _roles.GetAllAsync(ct);
        var questions = await _questions.GetActiveAsync(ct);
        var settings = await _settings.GetAsync(ct);

        return new PublicFormConfigDto
        {
            Roles = roles.Where(r => r.IsActive && ApplyableRoleIds.Contains(r.RoleId)).ToList(),
            Questions = questions.Select(MapQuestion).ToList(),
            RequirePhone = settings?.RequirePhone ?? true,
            RequireAddress = settings?.RequireAddress ?? true,
        };
    }

    public async Task<int> SubmitAsync(SubmitApplicationRequestDto request, CancellationToken ct = default)
    {
        if (!ApplyableRoleIds.Contains(request.RequestedRoleId))
            throw AppException.BadRequest("The selected role is not open for application.");

        var settings = await _settings.GetAsync(ct);
        var requirePhone = settings?.RequirePhone ?? true;
        var requireAddress = settings?.RequireAddress ?? true;

        if (requirePhone && string.IsNullOrWhiteSpace(request.Phone))
            throw AppException.BadRequest("Phone number is required.");
        if (requireAddress && string.IsNullOrWhiteSpace(request.Address))
            throw AppException.BadRequest("Address is required.");

        var activeQuestions = await _questions.GetActiveAsync(ct);
        var answersByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.AnswerText);

        foreach (var q in activeQuestions.Where(q => q.IsRequired))
        {
            answersByQuestion.TryGetValue(q.QuestionId, out var ans);
            if (string.IsNullOrWhiteSpace(ans))
                throw AppException.BadRequest($"Please answer: {q.QuestionText}");
        }

        var application = new RoleApplication
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            RequestedRoleId = request.RequestedRoleId,
        };
        var applicationId = await _applications.CreateAsync(application, ct);

        // Persist an answer row per active question (snapshotting the text).
        foreach (var q in activeQuestions)
        {
            answersByQuestion.TryGetValue(q.QuestionId, out var ans);
            await _applications.CreateAnswerAsync(new ApplicationAnswer
            {
                ApplicationId = applicationId,
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                AnswerText = ans,
            }, ct);
        }

        if (settings?.EmailNotificationsEnabled ?? true)
        {
            var roleName = (await _roles.GetByIdAsync(request.RequestedRoleId, ct))?.RoleName ?? "the requested role";
            await SendSubmissionEmailsAsync(request, roleName, settings?.NotificationEmail, ct);
        }

        return applicationId;
    }

    // ----------------------------------------------------------------- Review

    public async Task<IReadOnlyList<ApplicationListItemDto>> GetAllAsync(string? status, CancellationToken ct = default)
    {
        var apps = await _applications.GetAllAsync(status, ct);
        return apps.Select(a => new ApplicationListItemDto
        {
            ApplicationId = a.ApplicationId,
            FullName = a.FullName,
            Email = a.Email,
            RequestedRoleId = a.RequestedRoleId,
            RequestedRoleName = a.RequestedRoleName ?? string.Empty,
            Status = a.Status,
            CreatedOn = a.CreatedOn,
        }).ToList();
    }

    public async Task<ApplicationDetailDto?> GetByIdAsync(int applicationId, CancellationToken ct = default)
    {
        var app = await _applications.GetByIdAsync(applicationId, ct);
        if (app is null) return null;

        var answers = await _applications.GetAnswersAsync(applicationId, ct);
        return new ApplicationDetailDto
        {
            ApplicationId = app.ApplicationId,
            FullName = app.FullName,
            Email = app.Email,
            Phone = app.Phone,
            Address = app.Address,
            RequestedRoleId = app.RequestedRoleId,
            RequestedRoleName = app.RequestedRoleName ?? string.Empty,
            Status = app.Status,
            RejectionReason = app.RejectionReason,
            ReviewedOn = app.ReviewedOn,
            CreatedOn = app.CreatedOn,
            Answers = answers.Select(a => new ApplicationAnswerDto
            {
                QuestionId = a.QuestionId,
                QuestionText = a.QuestionText,
                AnswerText = a.AnswerText,
            }).ToList(),
        };
    }

    public async Task ApproveAsync(int applicationId, int reviewerUserId, int reviewerRoleId, Guid? officeId, CancellationToken ct = default)
    {
        var app = await _applications.GetByIdAsync(applicationId, ct)
            ?? throw AppException.NotFound("Application not found.");

        if (!string.Equals(app.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("This application has already been reviewed.");

        // Assign the new user's office. An Admin approver can only place people in
        // their own office; a SuperAdmin chooses the office (passed in).
        Guid? assignedOffice = officeId;
        if (reviewerRoleId == RoleConstants.AdminId)
        {
            var reviewer = await _users.GetByIdAsync(reviewerUserId, ct);
            assignedOffice = reviewer?.OfficeId;
        }

        var tempPassword = GenerateTemporaryPassword();
        await _users.CreateAsync(new CreateUserRequestDto
        {
            Email = app.Email,
            FullName = app.FullName,
            Password = tempPassword,
            RoleId = app.RequestedRoleId,
            OfficeId = assignedOffice,
        }, ct);

        await _applications.UpdateStatusAsync(applicationId, "Approved", null, reviewerUserId, ct);

        try
        {
            await _email.SendCredentialsAsync(app.Email, app.FullName, app.Email, tempPassword, BuildLoginUrl(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approved application {Id} but failed to send credentials email.", applicationId);
        }
    }

    public async Task RejectAsync(int applicationId, int reviewerUserId, string reason, CancellationToken ct = default)
    {
        var app = await _applications.GetByIdAsync(applicationId, ct)
            ?? throw AppException.NotFound("Application not found.");

        if (!string.Equals(app.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("This application has already been reviewed.");

        await _applications.UpdateStatusAsync(applicationId, "Rejected", reason, reviewerUserId, ct);

        try
        {
            await _email.SendApplicationRejectedAsync(app.Email, app.FullName, app.RequestedRoleName ?? "the requested role", reason, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rejected application {Id} but failed to send rejection email.", applicationId);
        }
    }

    // --------------------------------------------------------------- Settings

    public async Task<ApplicationSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var s = await _settings.GetAsync(ct);
        return new ApplicationSettingsDto
        {
            RequirePhone = s?.RequirePhone ?? true,
            RequireAddress = s?.RequireAddress ?? true,
            EmailNotificationsEnabled = s?.EmailNotificationsEnabled ?? true,
            NotificationEmail = s?.NotificationEmail,
            UpdatedOn = s?.UpdatedOn ?? DateTime.UtcNow,
        };
    }

    public async Task<ApplicationSettingsDto> UpsertSettingsAsync(UpsertApplicationSettingsDto request, CancellationToken ct = default)
    {
        var saved = await _settings.UpsertAsync(new ApplicationSettings
        {
            RequirePhone = request.RequirePhone,
            RequireAddress = request.RequireAddress,
            EmailNotificationsEnabled = request.EmailNotificationsEnabled,
            NotificationEmail = string.IsNullOrWhiteSpace(request.NotificationEmail) ? null : request.NotificationEmail.Trim(),
        }, ct);

        return new ApplicationSettingsDto
        {
            RequirePhone = saved.RequirePhone,
            RequireAddress = saved.RequireAddress,
            EmailNotificationsEnabled = saved.EmailNotificationsEnabled,
            NotificationEmail = saved.NotificationEmail,
            UpdatedOn = saved.UpdatedOn,
        };
    }

    public async Task<IReadOnlyList<QuestionDto>> GetQuestionsAsync(CancellationToken ct = default)
    {
        var questions = await _questions.GetAllAsync(ct);
        return questions.Select(MapQuestion).ToList();
    }

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionRequestDto request, CancellationToken ct = default)
    {
        var entity = new ApplicationQuestion
        {
            QuestionText = request.QuestionText,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            IsActive = true,
        };
        entity.QuestionId = await _questions.CreateAsync(entity, ct);
        return MapQuestion(entity);
    }

    public async Task<QuestionDto> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request, CancellationToken ct = default)
    {
        var existing = await _questions.GetByIdAsync(questionId, ct)
            ?? throw AppException.NotFound("Question not found.");

        existing.QuestionText = request.QuestionText;
        existing.IsRequired = request.IsRequired;
        existing.IsActive = request.IsActive;
        existing.SortOrder = request.SortOrder;

        await _questions.UpdateAsync(existing, ct);
        return MapQuestion(existing);
    }

    public Task DeactivateQuestionAsync(int questionId, CancellationToken ct = default)
        => _questions.DeactivateAsync(questionId, ct);

    // ----------------------------------------------------------------- Helpers

    private async Task SendSubmissionEmailsAsync(
        SubmitApplicationRequestDto request, string roleName, string? notificationEmail, CancellationToken ct)
    {
        try
        {
            await _email.SendApplicationReceivedAsync(request.Email, request.FullName, roleName, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send application-received email to {Email}.", request.Email);
        }

        // Notify reviewers: the configured address, otherwise all active admins.
        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(notificationEmail))
            recipients.Add(notificationEmail.Trim());

        try
        {
            var users = await _users.GetAllAsync(ct);
            foreach (var u in users.Where(u => u.IsActive &&
                         (u.RoleName == RoleConstants.SuperAdmin || u.RoleName == RoleConstants.Admin)))
            {
                recipients.Add(u.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin recipients for application notification.");
        }

        foreach (var to in recipients)
        {
            try
            {
                await _email.SendApplicationNotificationAsync(to, request.FullName, request.Email, roleName, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send application notification to {Email}.", to);
            }
        }
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

    /// <summary>A random 14-char password that satisfies the 8+ minimum with mixed classes.</summary>
    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(12);
        var core = new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        // Guarantee an upper, lower, digit and symbol regardless of the random draw.
        return core + "Aa9!";
    }

    private static QuestionDto MapQuestion(ApplicationQuestion q) => new()
    {
        QuestionId = q.QuestionId,
        QuestionText = q.QuestionText,
        IsRequired = q.IsRequired,
        IsActive = q.IsActive,
        SortOrder = q.SortOrder,
    };
}
