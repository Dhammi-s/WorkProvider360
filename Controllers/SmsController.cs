using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaS.Core.Constants;
using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Sends SMS to users via Twilio. SuperAdmin / Admin only. The destination is
/// resolved from an existing user's stored phone, or an explicit number.
/// </summary>
[Authorize(Roles = $"{RoleConstants.SuperAdmin},{RoleConstants.Admin}")]
public sealed class SmsController : BaseApiController
{
    private readonly ISmsService _sms;
    private readonly IUserService _users;

    public SmsController(ISmsService sms, IUserService users)
    {
        _sms = sms;
        _users = users;
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<object?>>> Send(
        [FromBody] SendSmsRequestDto request, CancellationToken ct)
    {
        var toNumber = request.ToNumber?.Trim();

        if (request.UserId is int userId)
        {
            var user = await _users.GetByIdAsync(userId, ct)
                ?? throw AppException.NotFound("User not found.");
            // Fall back to the stored phone when no explicit number was given.
            if (string.IsNullOrWhiteSpace(toNumber))
                toNumber = user.Phone;
        }

        if (string.IsNullOrWhiteSpace(toNumber))
            throw AppException.BadRequest("No phone number available. Add one to the user or enter a number.");

        var sid = await _sms.SendAsync(toNumber, request.Message, ct);
        return Ok(ApiResponse.Ok($"SMS sent ({sid})."));
    }
}
