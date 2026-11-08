using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

/// <summary>Resultado de geocodificación guardado para no repetir consultas al proveedor.</summary>
public class GeocodificacionCache : BaseEntity
{
    /// <summary>"r:lat,lon" (inversa, redondeada a 4 decimales ≈ 11 m) o "s:texto" (búsqueda).</summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>JSON con la dirección y sus coordenadas, almacenado como jsonb.</summary>
    public string Resultado { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
