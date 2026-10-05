/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-09-03
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

namespace SaaS.Core.Dtos.Outbound;

/// <summary>The client portal home dashboard.</summary>
public sealed class PortalDashboardDto
{
    public ClientVisitDto? NextVisit { get; set; }
    public IReadOnlyList<ClientVisitDto> TodayVisits { get; set; } = new List<ClientVisitDto>();
    public int UpcomingCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalHoursThisMonth { get; set; }
    public IReadOnlyList<ServiceTypeDto> Services { get; set; } = new List<ServiceTypeDto>();
}

/// <summary>The signed-in client's own profile (read + limited edit).</summary>
public sealed class PortalProfileDto
{
    public int ClientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? AccessInstructions { get; set; }
    public string Status { get; set; } = "Active";
    public IReadOnlyList<ServiceTypeDto> ServiceTypes { get; set; } = new List<ServiceTypeDto>();
}
