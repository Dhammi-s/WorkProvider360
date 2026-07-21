using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Role-application workflow: public form config + submission, SuperAdmin/Admin
/// review (approve/reject), and SuperAdmin-managed form settings + questions.
/// </summary>
public interface IApplicationService
{
    // Public (anonymous)
    Task<PublicFormConfigDto> GetPublicFormConfigAsync(CancellationToken ct = default);
    Task<int> SubmitAsync(SubmitApplicationRequestDto request, CancellationToken ct = default);

    // Review (SuperAdmin / Admin)
    Task<IReadOnlyList<ApplicationListItemDto>> GetAllAsync(string? status, CancellationToken ct = default);
    Task<ApplicationDetailDto?> GetByIdAsync(int applicationId, CancellationToken ct = default);
    Task ApproveAsync(int applicationId, int reviewerUserId, CancellationToken ct = default);
    Task RejectAsync(int applicationId, int reviewerUserId, string reason, CancellationToken ct = default);

    // Settings + questions (SuperAdmin)
    Task<ApplicationSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task<ApplicationSettingsDto> UpsertSettingsAsync(UpsertApplicationSettingsDto request, CancellationToken ct = default);
    Task<IReadOnlyList<QuestionDto>> GetQuestionsAsync(CancellationToken ct = default);
    Task<QuestionDto> CreateQuestionAsync(CreateQuestionRequestDto request, CancellationToken ct = default);
    Task<QuestionDto> UpdateQuestionAsync(int questionId, UpdateQuestionRequestDto request, CancellationToken ct = default);
    Task DeactivateQuestionAsync(int questionId, CancellationToken ct = default);
}
