/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;

namespace SaaS.Core.Interfaces.Services;

/// <summary>
/// Retrieval-augmented assistant that answers questions about WorkProvider360,
/// with per-user long-term chat memory persisted in the tenant database.
/// </summary>
public interface IChatbotService
{
    Task<ChatReplyDto> AskAsync(int userId, ChatRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(int userId, CancellationToken ct = default);
    Task ClearHistoryAsync(int userId, CancellationToken ct = default);
}
