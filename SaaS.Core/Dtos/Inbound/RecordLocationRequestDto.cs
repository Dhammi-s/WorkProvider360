using System.ComponentModel.DataAnnotations;

namespace SaaS.Core.Dtos.Inbound;

/// <summary>A GPS position posted by the assigned user's device.</summary>
public sealed class RecordLocationRequestDto
{
    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    /// <summary>Reported accuracy radius in metres, if the device supplied it.</summary>
    public decimal? AccuracyMeters { get; set; }
}
