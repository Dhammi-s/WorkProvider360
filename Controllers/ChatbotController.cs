/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// In-app assistant. Answers questions about WorkProvider360 for the signed-in
/// user and keeps a per-user chat history in the tenant database.
/// </summary>
[Authorize]
public sealed class ChatbotController : BaseApiController
{
    private readonly IChatbotService _chatbot;

    public ChatbotController(IChatbotService chatbot) => _chatbot = chatbot;

    [HttpPost("ask")]
    public async Task<ActionResult<ApiResponse<ChatReplyDto>>> Ask(
        [FromBody] ChatRequestDto request, CancellationToken ct)
    {
        var reply = await _chatbot.AskAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<ChatReplyDto>.Ok(reply));
    }

    /// <summary>The current user's saved chat history (oldest first).</summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatMessageDto>>>> History(CancellationToken ct)
    {
        var history = await _chatbot.GetHistoryAsync(CurrentUserId, ct);
        return Ok(ApiResponse<IReadOnlyList<ChatMessageDto>>.Ok(history));
    }

    /// <summary>Clear the current user's chat history.</summary>
    [HttpDelete("history")]
    public async Task<ActionResult<ApiResponse<object?>>> ClearHistory(CancellationToken ct)
    {
        await _chatbot.ClearHistoryAsync(CurrentUserId, ct);
        return Ok(ApiResponse.Ok("Chat history cleared."));
    }
}
