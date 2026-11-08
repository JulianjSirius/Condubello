using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

/// <summary>Ruta autorizada en la que estudiantes e instructores pueden operar.</summary>
public class Ruta : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>GeoJSON de la ruta autorizada, almacenado como jsonb.</summary>
    public string? GeoJson { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<SesionPractica> Sesiones { get; set; } = [];
}
