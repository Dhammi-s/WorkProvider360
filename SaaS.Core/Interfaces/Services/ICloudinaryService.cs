namespace SaaS.Core.Interfaces.Services;

/// <summary>Uploads images to Cloudinary and returns their hosted URL.</summary>
public interface ICloudinaryService
{
    bool IsConfigured { get; }

    /// <summary>
    /// Uploads a base64 image data URI to the given folder and returns the
    /// secure (https) URL. Throws if Cloudinary is not configured or rejects it.
    /// </summary>
    Task<string> UploadImageAsync(string dataUri, string folder, CancellationToken ct = default);
}
