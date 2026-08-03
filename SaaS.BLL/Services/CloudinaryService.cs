/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaS.Core.Exceptions;
using SaaS.Core.Interfaces.Services;
using SaaS.Core.Settings;

namespace SaaS.BLL.Services;

/// <summary>
/// Uploads images to Cloudinary using the official SDK (handles signing). The
/// incoming value is a base64 data URI (data:image/...;base64,....) from the
/// cropped image; it is decoded to bytes and uploaded.
/// </summary>
public sealed class CloudinaryService : ICloudinaryService
{
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

        var (bytes, extension) = DecodeDataUri(dataUri);

        var cloudinary = new Cloudinary(new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret))
        {
            Api = { Secure = true },
        };

        using var stream = new MemoryStream(bytes);
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription($"upload.{extension}", stream),
            Folder = folder,
            UniqueFilename = true,
            Overwrite = false,
        };

        UploadResult result;
        try
        {
            result = await cloudinary.UploadAsync(uploadParams, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary upload failed.");
            throw new AppException($"Could not reach the image host: {ex.Message}", 502);
        }

        if (result.Error is not null)
        {
            _logger.LogError("Cloudinary rejected upload: {Message}", result.Error.Message);
            throw AppException.BadRequest($"Image upload failed: {result.Error.Message}");
        }

        var url = result.SecureUrl?.ToString();
        if (string.IsNullOrEmpty(url))
            throw new AppException("The image host did not return a URL.", 502);
        return url;
    }

    /// <summary>Splits a "data:image/png;base64,AAAA" URI into raw bytes + a file extension.</summary>
    private static (byte[] Bytes, string Extension) DecodeDataUri(string dataUri)
    {
        var comma = dataUri.IndexOf(',');
        var header = comma >= 0 ? dataUri[..comma] : string.Empty;
        var base64 = comma >= 0 ? dataUri[(comma + 1)..] : dataUri;

        var extension = "png";
        var slash = header.IndexOf("image/", StringComparison.OrdinalIgnoreCase);
        if (slash >= 0)
        {
            var rest = header[(slash + 6)..];
            var semi = rest.IndexOf(';');
            var mime = (semi >= 0 ? rest[..semi] : rest).Trim();
            if (!string.IsNullOrEmpty(mime)) extension = mime == "jpeg" ? "jpg" : mime;
        }

        try
        {
            return (Convert.FromBase64String(base64), extension);
        }
        catch (FormatException)
        {
            throw AppException.BadRequest("The image data is not valid base64.");
        }
    }
}
