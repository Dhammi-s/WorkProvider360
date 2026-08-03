/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using SaaS.Core.Dtos.Inbound;
using SaaS.Core.Dtos.Outbound;
using SaaS.Core.Entities;
using SaaS.Core.Interfaces.Infrastructure;
using SaaS.Core.Interfaces.Repositories;
using SaaS.Core.Interfaces.Services;

namespace SaaS.BLL.Services;

/// <summary>
/// Login-page content: agency identity (name + logo) plus SuperAdmin-editable
/// marketing text. When no content row exists, sensible defaults are returned.
/// </summary>
public sealed class LoginContentService : ILoginContentService
{
    private readonly ILoginContentRepository _content;
    private readonly IBrandingRepository _branding;
    private readonly ITenantContext _tenant;

    public LoginContentService(ILoginContentRepository content, IBrandingRepository branding, ITenantContext tenant)
    {
        _content = content;
        _branding = branding;
        _tenant = tenant;
    }

    public async Task<PublicLoginPageDto> GetPublicAsync(CancellationToken ct = default)
    {
        var row = await _content.GetAsync(ct);
        var branding = await _branding.GetAsync(ct);
        var agencyName = _tenant.Agency?.AgencyName;

        return new PublicLoginPageDto
        {
            AgencyName = string.IsNullOrWhiteSpace(agencyName) ? "WorkProvider360" : agencyName!,
            Logo = branding?.LogoBase64,
            Content = Map(row),
        };
    }

    public async Task<LoginContentDto> GetForEditAsync(CancellationToken ct = default)
        => Map(await _content.GetAsync(ct));

    public async Task<LoginContentDto> UpdateAsync(UpdateLoginContentDto r, CancellationToken ct = default)
    {
        var saved = await _content.UpsertAsync(new LoginPageContent
        {
            HeadlineLead = Clean(r.HeadlineLead),
            HeadlineHighlight = Clean(r.HeadlineHighlight),
            HeadlineTrail = Clean(r.HeadlineTrail),
            Subtitle = Clean(r.Subtitle),
            Stat1Label = Clean(r.Stat1Label), Stat1Value = Clean(r.Stat1Value),
            Stat2Label = Clean(r.Stat2Label), Stat2Value = Clean(r.Stat2Value),
            Stat3Label = Clean(r.Stat3Label), Stat3Value = Clean(r.Stat3Value),
            QuoteText = Clean(r.QuoteText), QuoteAuthor = Clean(r.QuoteAuthor), QuoteRole = Clean(r.QuoteRole),
        }, ct);
        return Map(saved);
    }

    /// <summary>Entity → DTO, filling any blank field with the built-in default.</summary>
    private static LoginContentDto Map(LoginPageContent? c) => new()
    {
        HeadlineLead = Or(c?.HeadlineLead, "Field service"),
        HeadlineHighlight = Or(c?.HeadlineHighlight, "reimagined"),
        HeadlineTrail = Or(c?.HeadlineTrail, "with AI"),
        Subtitle = Or(c?.Subtitle, "Intelligent scheduling, real-time dispatch, and AI-powered insights for modern service businesses."),
        Stat1Label = Or(c?.Stat1Label, "Jobs Dispatched"),
        Stat1Value = Or(c?.Stat1Value, "1.2M+"),
        Stat2Label = Or(c?.Stat2Label, "Active Teams"),
        Stat2Value = Or(c?.Stat2Value, "2,400+"),
        Stat3Label = Or(c?.Stat3Label, "Uptime SLA"),
        Stat3Value = Or(c?.Stat3Value, "99.97%"),
        QuoteText = Or(c?.QuoteText, "WorkProvider360 cut our scheduling time by 70% and increased our first-time fix rate to 94%. It's transformed how we operate."),
        QuoteAuthor = Or(c?.QuoteAuthor, "Jordan Rivera"),
        QuoteRole = Or(c?.QuoteRole, "COO, ClearPath HVAC — Toronto, ON"),
    };

    private static string Or(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
