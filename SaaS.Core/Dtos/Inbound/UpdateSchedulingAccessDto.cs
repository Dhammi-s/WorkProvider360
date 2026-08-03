/* =============================================================================
   WorkProvider360 - Multi-tenant SaaS platform
   Developed by : Jasmeet Singh  (Full Stack Software Engineer)
   Date         : 2026-07-31
   NOTE TO DEVELOPERS: Do NOT change functionality without full knowledge of the
   SaaS architecture. PLEASE FIRST DISCUSS WITH SOFTWARE ENGINEER JASMEET SINGH.
   ============================================================================= */

using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>SuperAdmin-only: set how much scheduling access Admin/Manager get.</summary>
public sealed class UpdateSchedulingAccessDto
{
    /// <summary>"None", "Read" or "Write".</summary>
    [Required]
    public string AdminAccess { get; set; } = "Write";

    /// <summary>"None", "Read" or "Write".</summary>
    [Required]
    public string ManagerAccess { get; set; } = "Read";
}
