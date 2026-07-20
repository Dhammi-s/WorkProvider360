namespace SaaS.Core.Settings;

/// <summary>
/// SMTP configuration bound from the "Smtp" section of appsettings.
/// </summary>
public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "No Reply";

    /// <summary>Base URL of the front-end used to build password-reset links.</summary>
    public string ResetPasswordBaseUrl { get; set; } = string.Empty;
}
