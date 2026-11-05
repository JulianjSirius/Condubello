namespace Condubello.Application.Features.SesionesPracticas;

public class SesionPracticaValidator(
    IRepository<Estudiante> estudiantes,
    IRepository<Instructor> instructores,
    IRepository<SesionPractica> sesiones,
    IRepository<ControlHorasEstudiante> controlHoras)
{
    private const double DuracionMaximaRecomendadaHoras = 2;

    /// <summary>Devuelve un fallo si la sesión no es válida; si lo es, agrega las advertencias y devuelve null.</summary>
    public async Task<Result<SesionPracticaDto>?> ValidarAsync(
        Guid estudianteId,
        SesionPracticaRequestBase data,
        EstadoPrueba estado,
        Guid? sesionId,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var instructorId = data.InstructorId!.Value;

        if (!await estudiantes.ExistsAsync(e => e.Id == estudianteId, cancellationToken))
            return Result<SesionPracticaDto>.NotFound($"No se encontró el estudiante con id {estudianteId}.");

        if (!await instructores.ExistsAsync(i => i.Id == instructorId, cancellationToken))
            return Result<SesionPracticaDto>.NotFound($"No se encontró el instructor con id {instructorId}.");

        var inicio = data.FechaHoraInicio!.Value.ToUtc();
        var fin = data.FechaHoraFin?.ToUtc();

        if (fin <= inicio)
            return Result<SesionPracticaDto>.Invalid("fechaHoraFin", "La fecha/hora de fin debe ser posterior a la de inicio.");

        if (estado == EstadoPrueba.Completada && fin is null)
            return Result<SesionPracticaDto>.Invalid("fechaHoraFin", "Una sesión COMPLETADA debe tener fecha/hora de fin.");

        if (estado != EstadoPrueba.Cancelada)
        {
            var finEfectivo = fin ?? inicio.AddHours(1);

            if (await sesiones.ExistsAsync(s =>
                    s.Id != sesionId &&
                    s.InstructorId == instructorId &&
                    s.EstadoPrueba != EstadoPrueba.Cancelada &&
                    s.FechaHoraInicio < finEfectivo &&
                    (s.FechaHoraFin ?? s.FechaHoraInicio.AddHours(1)) > inicio, cancellationToken))
                return Result<SesionPracticaDto>.Conflict("instructorId", "El instructor ya tiene otra sesión en ese horario.");

            if (await sesiones.ExistsAsync(s =>
                    s.Id != sesionId &&
                    s.EstudianteId == estudianteId &&
                    s.EstadoPrueba != EstadoPrueba.Cancelada &&
                    s.FechaHoraInicio < finEfectivo &&
                    (s.FechaHoraFin ?? s.FechaHoraInicio.AddHours(1)) > inicio, cancellationToken))
                return Result<SesionPracticaDto>.Conflict("estudianteId", "El estudiante ya tiene otra sesión en ese horario.");
        }

        if (fin is not null && (fin.Value - inicio).TotalHours > DuracionMaximaRecomendadaHoras)
            warnings.Add($"La sesión dura {(fin.Value - inicio).TotalHours:0.##} horas, más de las {DuracionMaximaRecomendadaHoras} horas recomendadas por sesión.");

        if (estado == EstadoPrueba.FraudeDetectado)
            warnings.Add("La sesión quedó marcada como FRAUDE_DETECTADO y requiere revisión de un administrativo.");

        if (!await controlHoras.ExistsAsync(c => c.EstudianteId == estudianteId, cancellationToken))
            warnings.Add("El estudiante no tiene un control de horas registrado; estas horas no se contabilizarán.");
        else if (!await controlHoras.ExistsAsync(c => c.EstudianteId == estudianteId && c.HorasRealizadas < c.HorasRequeridas, cancellationToken))
            warnings.Add("El estudiante ya completó todas sus horas requeridas.");

        return null;
    }

    public static void Aplicar(SesionPractica entity, SesionPracticaRequestBase data)
    {
        entity.InstructorId = data.InstructorId!.Value;
        entity.FechaHoraInicio = data.FechaHoraInicio!.Value.ToUtc();
        entity.FechaHoraFin = data.FechaHoraFin?.ToUtc();
        entity.DuracionHoras = entity.FechaHoraFin is { } fin
            ? Math.Round((decimal)(fin - entity.FechaHoraInicio).TotalHours, 2)
            : null;
        entity.EstadoPrueba = data.EstadoPrueba ?? entity.EstadoPrueba;
        entity.CalificacionInstructor = data.CalificacionInstructor;
        entity.CalificacionAlumnoAInstructor = data.CalificacionAlumnoAInstructor;
    }
}
