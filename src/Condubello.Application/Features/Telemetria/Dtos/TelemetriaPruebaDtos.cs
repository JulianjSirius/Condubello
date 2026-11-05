using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

public class TelemetriaPruebaDto
{
    public Guid Id { get; set; }
    public Guid SesionId { get; set; }
    public JsonElement? ResumenTelemetria { get; set; }
    public JsonElement? RutaGps { get; set; }
    public bool AlertaDesviacion { get; set; }
}

public abstract class TelemetriaPruebaRequestBase
{
    /// <summary>Objeto JSON con el resumen (velocidades, frenadas, etc.).</summary>
    public JsonElement? ResumenTelemetria { get; set; }

    /// <summary>GeoJSON de la ruta (ej. LineString o FeatureCollection).</summary>
    public JsonElement? RutaGps { get; set; }

    public bool AlertaDesviacion { get; set; }
}

public class CreateTelemetriaPruebaRequest : TelemetriaPruebaRequestBase
{
    [Required(ErrorMessage = "La sesión es obligatoria.")]
    public Guid? SesionId { get; set; }
}

public class UpdateTelemetriaPruebaRequest : TelemetriaPruebaRequestBase;

public class ActualizarRutaGpsRequest
{
    /// <summary>GeoJSON de la ruta (ej. LineString o FeatureCollection). Null la elimina.</summary>
    public JsonElement? RutaGps { get; set; }
}
