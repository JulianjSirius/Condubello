namespace Condubello.Application.Features.ControlHoras;

internal static class ControlHorasReglas
{
    public static List<string> Advertencias(ControlHorasEstudiante control)
    {
        var warnings = new List<string>();
        if (control.HorasRealizadas > control.HorasRequeridas)
            warnings.Add($"Las horas realizadas ({control.HorasRealizadas}) superan las {control.HorasRequeridas} horas requeridas para la licencia {control.TipoLicencia}.");
        else if (control.HorasRealizadas == control.HorasRequeridas)
            warnings.Add($"El estudiante ya completó las {control.HorasRequeridas} horas requeridas para la licencia {control.TipoLicencia}.");
        return warnings;
    }

    public static string Conflicto(string tipoLicencia) =>
        $"El estudiante ya tiene un control de horas para la licencia {tipoLicencia}.";
}

[Autorizar(Rol.Administrativo)]
public record CreateControlHorasCommand(CreateControlHorasRequest Data) : IRequest<Result<ControlHorasDto>>;

public class CreateControlHorasHandler(
    IRepository<ControlHorasEstudiante> repository,
    IRepository<Estudiante> estudiantes,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateControlHorasCommand, Result<ControlHorasDto>>
{
    public async Task<Result<ControlHorasDto>> Handle(CreateControlHorasCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var estudianteId = data.EstudianteId!.Value;

        if (!await estudiantes.ExistsAsync(e => e.Id == estudianteId, cancellationToken))
            return Result<ControlHorasDto>.NotFound($"No se encontró el estudiante con id {estudianteId}.");

        var tipoLicencia = data.TipoLicencia.Trim().ToUpperInvariant();
        if (await repository.ExistsAsync(c => c.EstudianteId == estudianteId && c.TipoLicencia == tipoLicencia, cancellationToken))
            return Result<ControlHorasDto>.Conflict("tipoLicencia", ControlHorasReglas.Conflicto(tipoLicencia));

        var entity = new ControlHorasEstudiante
        {
            EstudianteId = estudianteId,
            TipoLicencia = tipoLicencia,
            HorasRequeridas = data.HorasRequeridas,
            HorasRealizadas = data.HorasRealizadas
        };

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControlHorasDto>.Success(mapper.Map<ControlHorasDto>(entity), ControlHorasReglas.Advertencias(entity));
    }
}

[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record UpdateControlHorasCommand(Guid Id, UpdateControlHorasRequest Data) : IRequest<Result<ControlHorasDto>>;

/// <summary>El instructor solo puede actualizar el control de horas de sus estudiantes (con al menos una sesión con él).</summary>
public class UpdateControlHorasHandler(
    IRepository<ControlHorasEstudiante> repository,
    IRepository<SesionPractica> sesiones,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateControlHorasCommand, Result<ControlHorasDto>>
{
    public async Task<Result<ControlHorasDto>> Handle(UpdateControlHorasCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<ControlHorasDto>.NotFound($"No se encontró el control de horas con id {request.Id}.");

        if (currentUser.EsInstructor)
        {
            var yo = currentUser.Id;
            if (!await sesiones.ExistsAsync(s => s.EstudianteId == entity.EstudianteId && s.InstructorId == yo, cancellationToken))
                return Result<ControlHorasDto>.Forbidden("Solo puede modificar el control de horas de sus estudiantes.");
        }

        var data = request.Data;
        var tipoLicencia = data.TipoLicencia.Trim().ToUpperInvariant();
        if (await repository.ExistsAsync(
                c => c.Id != entity.Id && c.EstudianteId == entity.EstudianteId && c.TipoLicencia == tipoLicencia,
                cancellationToken))
            return Result<ControlHorasDto>.Conflict("tipoLicencia", ControlHorasReglas.Conflicto(tipoLicencia));

        entity.TipoLicencia = tipoLicencia;
        entity.HorasRequeridas = data.HorasRequeridas;
        entity.HorasRealizadas = data.HorasRealizadas;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControlHorasDto>.Success(mapper.Map<ControlHorasDto>(entity), ControlHorasReglas.Advertencias(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteControlHorasCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteControlHorasHandler(IRepository<ControlHorasEstudiante> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteControlHorasCommand, ControlHorasEstudiante>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el control de horas.";
}
