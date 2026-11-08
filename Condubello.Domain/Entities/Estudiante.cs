using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class Estudiante : Persona
{
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>Las cuentas autorregistradas quedan en false hasta que un administrativo las aprueba.</summary>
    public bool Aprobado { get; set; } = true;

    public ICollection<ControlHorasEstudiante> ControlHoras { get; set; } = [];
    public ICollection<SesionPractica> Sesiones { get; set; } = [];
}
