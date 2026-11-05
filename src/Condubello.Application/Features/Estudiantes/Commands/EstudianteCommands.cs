using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Estudiantes;

[Autorizar(Rol.Administrativo)]
public record CreateEstudianteCommand(CreateEstudianteRequest Data) : IRequest<Result<EstudianteDto>>, IPersonaCommand
{
    IPersonaRequest IPersonaCommand.Data => Data;
}

public class CreateEstudianteHandler(
    IRepository<Estudiante> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : CreatePersonaHandler<CreateEstudianteCommand, Estudiante, EstudianteDto>(repository, validator, passwordHasher, unitOfWork, mapper)
{
    protected override Rol Rol => Rol.Estudiante;
}

/// <summary>El administrativo no edita datos personales del estudiante; solo puede restablecer su contraseña.</summary>
[Autorizar(Rol.Administrativo)]
public record RestablecerPasswordEstudianteCommand(Guid Id, RestablecerPasswordRequest Data)
    : IRequest<Result<EstudianteDto>>, IByIdRequest;

public class RestablecerPasswordEstudianteHandler(
    IRepository<Estudiante> repository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RestablecerPasswordEstudianteCommand, Result<EstudianteDto>>
{
    public async Task<Result<EstudianteDto>> Handle(RestablecerPasswordEstudianteCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<EstudianteDto>.NotFound($"No se encontró el estudiante con id {request.Id}.");

        entity.PasswordHash = passwordHasher.Hash(request.Data.PasswordNueva);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EstudianteDto>.Success(mapper.Map<EstudianteDto>(entity));
    }
}

/// <summary>El estudiante autenticado solo puede cambiar su email y su contraseña.</summary>
[Autorizar(Rol.Estudiante)]
public record ActualizarMiCuentaCommand(ActualizarMiCuentaRequest Data) : IRequest<Result<EstudianteDto>>;

public class ActualizarMiCuentaHandler(
    IRepository<Estudiante> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ActualizarMiCuentaCommand, Result<EstudianteDto>>
{
    public async Task<Result<EstudianteDto>> Handle(ActualizarMiCuentaCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(currentUser.Id, cancellationToken);
        if (entity is null)
            return Result<EstudianteDto>.NotFound("No se encontró el estudiante autenticado.");

        var data = request.Data;
        if (string.IsNullOrWhiteSpace(data.Email) && string.IsNullOrWhiteSpace(data.PasswordNueva))
            return Result<EstudianteDto>.Invalid("email", "Debe enviar un nuevo email o una nueva contraseña.");

        if (!passwordHasher.Verify(data.PasswordActual, entity.PasswordHash))
            return Result<EstudianteDto>.Invalid("passwordActual", "La contraseña actual es incorrecta.");

        if (!string.IsNullOrWhiteSpace(data.Email))
        {
            var errores = await validator.ValidarUnicidadAsync(Rol.Estudiante, entity.Cedula, data.Email, entity.Id, cancellationToken);
            if (errores.Count > 0)
                return Result<EstudianteDto>.Conflict(errores);

            entity.Email = PersonaValidator.NormalizarEmail(data.Email);
        }

        if (!string.IsNullOrWhiteSpace(data.PasswordNueva))
            entity.PasswordHash = passwordHasher.Hash(data.PasswordNueva);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EstudianteDto>.Success(mapper.Map<EstudianteDto>(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteEstudianteCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteEstudianteHandler(
    IRepository<Estudiante> repository,
    IRepository<SesionPractica> sesiones,
    IRepository<ControlHorasEstudiante> controlHoras,
    IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteEstudianteCommand, Estudiante>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el estudiante.";

    protected override async Task<Result<Guid>?> ValidateAsync(
        Estudiante entity, List<string> warnings, CancellationToken cancellationToken)
    {
        if (await sesiones.ExistsAsync(s => s.EstudianteId == entity.Id, cancellationToken))
            return Result<Guid>.Conflict("id", "No se puede eliminar el estudiante porque tiene sesiones prácticas registradas.");

        if (await controlHoras.ExistsAsync(c => c.EstudianteId == entity.Id, cancellationToken))
            warnings.Add("También se eliminaron los registros de control de horas del estudiante.");

        return null;
    }
}
