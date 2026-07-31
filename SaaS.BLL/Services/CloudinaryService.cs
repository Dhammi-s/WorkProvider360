using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Signed Cloudinary uploads over a plain HttpClient (no SDK dependency).
/// The signature is SHA-1 of the alphabetically-sorted params + the API secret.
/// </summary>
public sealed class CloudinaryService : ICloudinaryService
{
    private static readonly HttpClient _http = new();

    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => _settings.IsConfigured;

    public async Task<string> UploadImageAsync(string dataUri, string folder, CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
            throw new AppException("Image hosting is not configured. Set the Cloudinary CloudName, ApiKey and ApiSecret.", 503);
        if (string.IsNullOrWhiteSpace(dataUri))
            throw AppException.BadRequest("No image was provided.");

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Params to sign (everything except file, api_key, signature), sorted alphabetically.
        var signature = Sha1Hex($"folder={folder}&timestamp={timestamp}{_settings.ApiSecret}");

        using var form = new MultipartFormDataContent
        {
            { new StringContent(dataUri), "file" },
            { new StringContent(_settings.ApiKey), "api_key" },
            { new StringContent(timestamp), "timestamp" },
            { new StringContent(folder), "folder" },
            { new StringContent(signature), "signature" },
        };

        var url = $"https://api.cloudinary.com/v1_1/{_settings.CloudName}/image/upload";
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsync(url, form, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary upload request failed.");
            throw new AppException($"Could not reach the image host: {ex.Message}", 502);
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cloudinary rejected upload: {Status} {Body}", (int)response.StatusCode, payload);
            throw AppException.BadRequest($"Image upload failed: {ReadError(payload)}");
        }

        var secureUrl = ReadString(payload, "secure_url");
        if (string.IsNullOrEmpty(secureUrl))
            throw new AppException("The image host did not return a URL.", 502);
        return secureUrl;
    }

    private static string Sha1Hex(string value)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(value));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static string ReadString(string json, string prop)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(prop, out var v) ? v.GetString() ?? string.Empty : string.Empty;
        }
        catch { return string.Empty; }
    }

    private static string ReadError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("error", out var e) && e.TryGetProperty("message", out var m)
                ? m.GetString() ?? "unknown error"
                : "unknown error";
        }
        catch { return "unknown error"; }
    }
}
