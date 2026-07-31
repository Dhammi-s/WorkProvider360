/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>SuperAdmin edit of the login page's left-panel content.</summary>
public sealed class UpdateLoginContentDto
{
    [MaxLength(100)] public string? HeadlineLead { get; set; }
    [MaxLength(100)] public string? HeadlineHighlight { get; set; }
    [MaxLength(100)] public string? HeadlineTrail { get; set; }
    [MaxLength(500)] public string? Subtitle { get; set; }
    [MaxLength(60)] public string? Stat1Label { get; set; }
    [MaxLength(40)] public string? Stat1Value { get; set; }
    [MaxLength(60)] public string? Stat2Label { get; set; }
    [MaxLength(40)] public string? Stat2Value { get; set; }
    [MaxLength(60)] public string? Stat3Label { get; set; }
    [MaxLength(40)] public string? Stat3Value { get; set; }
    [MaxLength(600)] public string? QuoteText { get; set; }
    [MaxLength(100)] public string? QuoteAuthor { get; set; }
    [MaxLength(150)] public string? QuoteRole { get; set; }
}
