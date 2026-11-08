using Condubello.Application.Features.Estudiantes;
using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Auth;

/// <summary>
/// Autorregistro público: crea un estudiante PENDIENTE de aprobación (no puede iniciar sesión hasta que
/// un administrativo lo apruebe). El rol no es elegible por quien se registra.
/// Sin [Autorizar], por lo que puede invocarse sin sesión.
/// </summary>
public record RegistrarEstudianteCommand(CreateEstudianteRequest Data) : IRequest<Result<EstudianteDto>>;

public class RegistrarEstudianteHandler(
    IRepository<Estudiante> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RegistrarEstudianteCommand, Result<EstudianteDto>>
{
    public async Task<Result<EstudianteDto>> Handle(RegistrarEstudianteCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;

        var errores = await validator.ValidarUnicidadAsync(Rol.Estudiante, data.Cedula, data.Email, null, cancellationToken);
        if (errores.Count > 0)
            return Result<EstudianteDto>.Conflict(errores);

        var entity = mapper.Map<Estudiante>(data);
        PersonaValidator.Normalizar(entity);
        entity.PasswordHash = passwordHasher.Hash(data.Password);
        entity.Aprobado = false;

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EstudianteDto>.Success(mapper.Map<EstudianteDto>(entity));
    }
}
