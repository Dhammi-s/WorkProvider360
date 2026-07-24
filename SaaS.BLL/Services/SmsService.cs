using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Sends SMS through the Twilio REST API using a plain HttpClient (no SDK
/// dependency). Credentials come from <see cref="TwilioSettings"/>.
/// </summary>
public sealed class SmsService : ISmsService
{
    // Single shared client (recommended reuse pattern — avoids socket exhaustion).
    private static readonly HttpClient _http = new();

    private readonly TwilioSettings _settings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IOptions<TwilioSettings> options, ILogger<SmsService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> SendAsync(string toNumber, string message, CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
            throw new AppException("SMS is not configured. Set the Twilio AccountSid, AuthToken and FromNumber.", 503);

        if (string.IsNullOrWhiteSpace(toNumber))
            throw AppException.BadRequest("A destination phone number is required.");
        if (string.IsNullOrWhiteSpace(message))
            throw AppException.BadRequest("The SMS message cannot be empty.");

        var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = toNumber.Trim(),
            ["From"] = _settings.FromNumber,
            ["Body"] = message,
        });

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio request failed for {To}", toNumber);
            throw new AppException($"Could not reach the SMS provider: {ex.Message}", 502);
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var detail = TryReadError(payload);
            _logger.LogError("Twilio rejected SMS to {To}: {Status} {Detail}", toNumber, (int)response.StatusCode, detail);
            throw AppException.BadRequest($"SMS could not be sent: {detail}");
        }

        var sid = TryReadSid(payload);
        _logger.LogInformation("SMS sent to {To} (sid {Sid})", toNumber, sid);
        return sid;
    }

    private static string TryReadError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? "unknown error" : "unknown error";
        }
        catch
        {
            return "unknown error";
        }
    }

    private static string TryReadSid(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("sid", out var s) ? s.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
