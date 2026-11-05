using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class Estudiante : Persona
{
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<ControlHorasEstudiante> ControlHoras { get; set; } = [];
    public ICollection<SesionPractica> Sesiones { get; set; } = [];
}
