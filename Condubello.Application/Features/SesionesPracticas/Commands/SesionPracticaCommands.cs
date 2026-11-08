namespace Condubello.Application.Features.SesionesPracticas;

[Autorizar(Rol.Administrativo)]
public record CreateSesionPracticaCommand(CreateSesionPracticaRequest Data) : IRequest<Result<SesionPracticaDto>>;

public class CreateSesionPracticaHandler(
    IRepository<SesionPractica> repository,
    SesionPracticaValidator validator,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateSesionPracticaCommand, Result<SesionPracticaDto>>
{
    public async Task<Result<SesionPracticaDto>> Handle(CreateSesionPracticaCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var estudianteId = data.EstudianteId!.Value;
        var estado = data.EstadoPrueba ?? EstadoPrueba.Programada;
        var warnings = new List<string>();

        var failure = await validator.ValidarAsync(estudianteId, data, estado, null, warnings, cancellationToken);
        if (failure is not null)
            return failure;

        var entity = new SesionPractica { EstudianteId = estudianteId };
        SesionPracticaValidator.Aplicar(entity, data);

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SesionPracticaDto>.Success(mapper.Map<SesionPracticaDto>(entity), warnings);
    }
}

[Autorizar(Rol.Administrativo)]
public record UpdateSesionPracticaCommand(Guid Id, UpdateSesionPracticaRequest Data) : IRequest<Result<SesionPracticaDto>>;

public class UpdateSesionPracticaHandler(
    IRepository<SesionPractica> repository,
    SesionPracticaValidator validator,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateSesionPracticaCommand, Result<SesionPracticaDto>>
{
    public async Task<Result<SesionPracticaDto>> Handle(UpdateSesionPracticaCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<SesionPracticaDto>.NotFound($"No se encontró la sesión práctica con id {request.Id}.");

        var data = request.Data;
        var estado = data.EstadoPrueba ?? entity.EstadoPrueba;
        var warnings = new List<string>();

        var failure = await validator.ValidarAsync(entity.EstudianteId, data, estado, entity.Id, warnings, cancellationToken);
        if (failure is not null)
            return failure;

        SesionPracticaValidator.Aplicar(entity, data);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SesionPracticaDto>.Success(mapper.Map<SesionPracticaDto>(entity), warnings);
    }
}

/// <summary>
/// Actualiza solo fechas, estado y calificación del instructor (la duración se recalcula).
/// El instructor solo puede actualizar sus propias sesiones.
/// </summary>
[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record ActualizarSeguimientoSesionCommand(Guid Id, ActualizarSeguimientoSesionRequest Data)
    : IRequest<Result<SesionPracticaDto>>;

public class ActualizarSeguimientoSesionHandler(
    IRepository<SesionPractica> repository,
    SesionPracticaValidator validator,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ActualizarSeguimientoSesionCommand, Result<SesionPracticaDto>>
{
    public async Task<Result<SesionPracticaDto>> Handle(ActualizarSeguimientoSesionCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<SesionPracticaDto>.NotFound($"No se encontró la sesión práctica con id {request.Id}.");

        if (currentUser.EsInstructor && entity.InstructorId != currentUser.Id)
            return Result<SesionPracticaDto>.Forbidden("Solo puede modificar sus propias sesiones prácticas.");

        // Los campos que no se pueden editar aquí conservan su valor actual.
        var data = new UpdateSesionPracticaRequest
        {
            InstructorId = entity.InstructorId,
            FechaHoraInicio = request.Data.FechaHoraInicio,
            FechaHoraFin = request.Data.FechaHoraFin,
            EstadoPrueba = request.Data.EstadoPrueba,
            CalificacionInstructor = request.Data.CalificacionInstructor,
            CalificacionAlumnoAInstructor = entity.CalificacionAlumnoAInstructor
        };
        var estado = data.EstadoPrueba ?? entity.EstadoPrueba;
        var warnings = new List<string>();

        var failure = await validator.ValidarAsync(entity.EstudianteId, data, estado, entity.Id, warnings, cancellationToken);
        if (failure is not null)
            return failure;

        SesionPracticaValidator.Aplicar(entity, data);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SesionPracticaDto>.Success(mapper.Map<SesionPracticaDto>(entity), warnings);
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteSesionPracticaCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteSesionPracticaHandler(IRepository<SesionPractica> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteSesionPracticaCommand, SesionPractica>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró la sesión práctica.";

    protected override Task<Result<Guid>?> ValidateAsync(
        SesionPractica entity, List<string> warnings, CancellationToken cancellationToken)
    {
        if (entity.EstadoPrueba == EstadoPrueba.Completada)
            warnings.Add("Se eliminó una sesión COMPLETADA; revise el control de horas del estudiante.");

        warnings.Add("También se eliminaron sus validaciones antifraude y registros de telemetría.");
        return Task.FromResult<Result<Guid>?>(null);
    }
}
