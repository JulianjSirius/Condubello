using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Condubello.Application.Features.Rutas;

public class RutaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public JsonElement? GeoJson { get; set; }
    public bool Activa { get; set; }
}

public abstract class RutaRequestBase
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>GeoJSON de la ruta autorizada (ej. LineString o FeatureCollection).</summary>
    public JsonElement? GeoJson { get; set; }

    public bool Activa { get; set; } = true;
}

public class CreateRutaRequest : RutaRequestBase;

public class UpdateRutaRequest : RutaRequestBase;
