using System.ComponentModel.DataAnnotations;

namespace Condubello.Application.Features.SesionesPracticas;

public class SesionPracticaDto
{
    public Guid Id { get; set; }
    public Guid EstudianteId { get; set; }
    public Guid InstructorId { get; set; }
    public Guid? RutaId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime? FechaHoraFin { get; set; }
    public decimal? DuracionHoras { get; set; }
    public EstadoPrueba EstadoPrueba { get; set; }
    public int? CalificacionInstructor { get; set; }
    public int? CalificacionAlumnoAInstructor { get; set; }
}

public abstract class SesionPracticaRequestBase
{
    [Required(ErrorMessage = "El instructor es obligatorio.")]
    public Guid? InstructorId { get; set; }

    /// <summary>Ruta autorizada asignada a la sesión (opcional).</summary>
    public Guid? RutaId { get; set; }

    [Required(ErrorMessage = "La fecha/hora de inicio es obligatoria.")]
    public DateTime? FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }

    /// <summary>PROGRAMADA, EN_CURSO, COMPLETADA, CANCELADA o FRAUDE_DETECTADO.</summary>
    public EstadoPrueba? EstadoPrueba { get; set; }

    [Range(1, 5, ErrorMessage = "La calificación del instructor debe estar entre 1 y 5.")]
    public int? CalificacionInstructor { get; set; }

    [Range(1, 5, ErrorMessage = "La calificación del alumno al instructor debe estar entre 1 y 5.")]
    public int? CalificacionAlumnoAInstructor { get; set; }
}

public class CreateSesionPracticaRequest : SesionPracticaRequestBase
{
    [Required(ErrorMessage = "El estudiante es obligatorio.")]
    public Guid? EstudianteId { get; set; }
}

public class UpdateSesionPracticaRequest : SesionPracticaRequestBase;

public class ActualizarSeguimientoSesionRequest
{
    [Required(ErrorMessage = "La fecha/hora de inicio es obligatoria.")]
    public DateTime? FechaHoraInicio { get; set; }

    public DateTime? FechaHoraFin { get; set; }

    /// <summary>PROGRAMADA, EN_CURSO, COMPLETADA, CANCELADA o FRAUDE_DETECTADO. Si no se envía, se conserva.</summary>
    public EstadoPrueba? EstadoPrueba { get; set; }

    [Range(1, 5, ErrorMessage = "La calificación del instructor debe estar entre 1 y 5.")]
    public int? CalificacionInstructor { get; set; }
}
