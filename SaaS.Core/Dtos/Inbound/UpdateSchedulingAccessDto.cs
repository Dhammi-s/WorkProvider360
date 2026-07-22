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
