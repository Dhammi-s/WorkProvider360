/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>Receives support messages from signed-in users and emails the support inbox.</summary>
[Authorize]
public sealed class SupportController : BaseApiController
{
    /// <summary>Where support requests are delivered.</summary>
    private const string SupportInbox = "workprovider360.com@gmail.com";

    private readonly IEmailService _email;
    private readonly IUserService _users;

    public SupportController(IEmailService email, IUserService users)
    {
        _email = email;
        _users = users;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object?>>> Send(
        [FromBody] SupportRequestDto request, CancellationToken ct)
    {
        var me = await _users.GetByIdAsync(CurrentUserId, ct);
        var name = WebUtility.HtmlEncode(me?.FullName ?? "A user");
        var email = WebUtility.HtmlEncode(me?.Email ?? "(unknown)");
        var role = WebUtility.HtmlEncode(me?.RoleName ?? string.Empty);
        var subject = WebUtility.HtmlEncode(request.Subject.Trim());
        var message = WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br/>");

        var body =
            $"<h2 style='font-family:Arial'>New support request</h2>" +
            $"<p style='font-family:Arial'><strong>From:</strong> {name} &lt;{email}&gt; ({role}) · Agency #{CurrentAgencyId}</p>" +
            $"<p style='font-family:Arial'><strong>Subject:</strong> {subject}</p>" +
            $"<div style='font-family:Arial;border-left:3px solid #6366f1;padding-left:12px;color:#334155'>{message}</div>";

        await _email.SendAsync(SupportInbox, $"[Support] {request.Subject.Trim()}", body, ct);
        return Ok(ApiResponse.Ok("Your message has been sent to our support team."));
    }
}
