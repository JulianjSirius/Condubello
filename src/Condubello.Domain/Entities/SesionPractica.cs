using Condubello.Domain.Common;
using Condubello.Domain.Enums;

namespace Condubello.Domain.Entities;

public class SesionPractica : BaseEntity
{
    public Guid EstudianteId { get; set; }
    public Guid InstructorId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public decimal? DuracionHoras { get; set; }
    public EstadoPrueba EstadoPrueba { get; set; } = EstadoPrueba.Programada;
    public int? CalificacionInstructor { get; set; }
    public int? CalificacionAlumnoAInstructor { get; set; }

    public Estudiante Estudiante { get; set; } = null!;
    public Instructor Instructor { get; set; } = null!;
    public ICollection<ValidacionAntifraude> ValidacionesAntifraude { get; set; } = [];
    public ICollection<TelemetriaPrueba> Telemetrias { get; set; } = [];
}
