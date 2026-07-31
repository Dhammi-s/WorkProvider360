namespace SaaS.Core.Entities;

/// <summary>Single-row (SettingsId = 1) editable marketing content for the login page.</summary>
public sealed class LoginPageContent
{
    public int SettingsId { get; set; }
    public string? HeadlineLead { get; set; }
    public string? HeadlineHighlight { get; set; }
    public string? HeadlineTrail { get; set; }
    public string? Subtitle { get; set; }
    public string? Stat1Label { get; set; }
    public string? Stat1Value { get; set; }
    public string? Stat2Label { get; set; }
    public string? Stat2Value { get; set; }
    public string? Stat3Label { get; set; }
    public string? Stat3Value { get; set; }
    public string? QuoteText { get; set; }
    public string? QuoteAuthor { get; set; }
    public string? QuoteRole { get; set; }
    public DateTime UpdatedOn { get; set; }
}
