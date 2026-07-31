namespace SaaS.Core.Dtos.Outbound;

/// <summary>A single page of results plus the total row count (for server-side paging).</summary>
public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
