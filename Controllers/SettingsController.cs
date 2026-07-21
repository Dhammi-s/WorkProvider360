using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// SuperAdmin-only configuration of the public application form: mandatory-field
/// toggles, email notifications, and the custom screening questions.
/// </summary>
[Authorize(Roles = RoleConstants.SuperAdmin)]
public sealed class SettingsController : BaseApiController
{
    private readonly IApplicationService _applications;

    public SettingsController(IApplicationService applications) => _applications = applications;

    [HttpGet("application")]
    public async Task<ActionResult<ApiResponse<ApplicationSettingsDto>>> GetSettings(CancellationToken ct)
    {
        var settings = await _applications.GetSettingsAsync(ct);
        return Ok(ApiResponse<ApplicationSettingsDto>.Ok(settings));
    }

    [HttpPut("application")]
    public async Task<ActionResult<ApiResponse<ApplicationSettingsDto>>> UpdateSettings(
        [FromBody] UpsertApplicationSettingsDto request, CancellationToken ct)
    {
        var settings = await _applications.UpsertSettingsAsync(request, ct);
        return Ok(ApiResponse<ApplicationSettingsDto>.Ok(settings, "Settings saved."));
    }

    [HttpGet("application/questions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuestionDto>>>> GetQuestions(CancellationToken ct)
    {
        var questions = await _applications.GetQuestionsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<QuestionDto>>.Ok(questions));
    }

    [HttpPost("application/questions")]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> CreateQuestion(
        [FromBody] CreateQuestionRequestDto request, CancellationToken ct)
    {
        var created = await _applications.CreateQuestionAsync(request, ct);
        return Ok(ApiResponse<QuestionDto>.Ok(created, "Question added."));
    }

    [HttpPut("application/questions/{id:int}")]
    public async Task<ActionResult<ApiResponse<QuestionDto>>> UpdateQuestion(
        int id, [FromBody] UpdateQuestionRequestDto request, CancellationToken ct)
    {
        var updated = await _applications.UpdateQuestionAsync(id, request, ct);
        return Ok(ApiResponse<QuestionDto>.Ok(updated, "Question updated."));
    }

    [HttpDelete("application/questions/{id:int}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteQuestion(int id, CancellationToken ct)
    {
        await _applications.DeactivateQuestionAsync(id, ct);
        return Ok(ApiResponse.Ok("Question removed."));
    }
}
