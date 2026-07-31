/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>A single page of results plus the total row count (for server-side paging).</summary>
public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
