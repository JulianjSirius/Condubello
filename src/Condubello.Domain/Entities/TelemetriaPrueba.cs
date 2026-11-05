using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class TelemetriaPrueba : BaseEntity
{
    public Guid SesionId { get; set; }

    /// <summary>JSON almacenado como jsonb.</summary>
    public string? ResumenTelemetria { get; set; }

    /// <summary>GeoJSON almacenado como jsonb.</summary>
    public string? RutaGps { get; set; }

    public bool AlertaDesviacion { get; set; }

    public SesionPractica Sesion { get; set; } = null!;
}
