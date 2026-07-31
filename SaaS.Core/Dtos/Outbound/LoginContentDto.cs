namespace SaaS.Core.Dtos.Outbound;

/// <summary>The editable marketing fields shown on the login page's left panel.</summary>
public sealed class LoginContentDto
{
    public string HeadlineLead { get; set; } = string.Empty;
    public string HeadlineHighlight { get; set; } = string.Empty;
    public string HeadlineTrail { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Stat1Label { get; set; } = string.Empty;
    public string Stat1Value { get; set; } = string.Empty;
    public string Stat2Label { get; set; } = string.Empty;
    public string Stat2Value { get; set; } = string.Empty;
    public string Stat3Label { get; set; } = string.Empty;
    public string Stat3Value { get; set; } = string.Empty;
    public string QuoteText { get; set; } = string.Empty;
    public string QuoteAuthor { get; set; } = string.Empty;
    public string QuoteRole { get; set; } = string.Empty;
}

/// <summary>Everything the anonymous login page renders: agency identity + content.</summary>
public sealed class PublicLoginPageDto
{
    public string AgencyName { get; set; } = "WorkProvider360";
    public string? Logo { get; set; }
    public LoginContentDto Content { get; set; } = new();
}
