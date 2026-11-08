using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Administrativos;

[Autorizar(Rol.Administrativo)]
public record CreateAdministrativoCommand(CreateAdministrativoRequest Data)
    : IRequest<Result<AdministrativoDto>>, IPersonaCommand
{
    IPersonaRequest IPersonaCommand.Data => Data;
}

public class CreateAdministrativoHandler(
    IRepository<Administrativo> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : CreatePersonaHandler<CreateAdministrativoCommand, Administrativo, AdministrativoDto>(repository, validator, passwordHasher, unitOfWork, mapper)
{
    protected override Rol Rol => Rol.Administrativo;
}

[Autorizar(Rol.Administrativo)]
public record UpdateAdministrativoCommand(Guid Id, UpdateAdministrativoRequest Data)
    : IRequest<Result<AdministrativoDto>>, IPersonaCommand, IByIdRequest
{
    IPersonaRequest IPersonaCommand.Data => Data;
}

public class UpdateAdministrativoHandler(
    IRepository<Administrativo> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : UpdatePersonaHandler<UpdateAdministrativoCommand, Administrativo, AdministrativoDto>(repository, validator, passwordHasher, unitOfWork, mapper)
{
    protected override Rol Rol => Rol.Administrativo;
}

[Autorizar(Rol.Administrativo)]
public record DeleteAdministrativoCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteAdministrativoHandler(IRepository<Administrativo> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteAdministrativoCommand, Administrativo>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el administrativo.";

    protected override async Task<Result<Guid>?> ValidateAsync(
        Administrativo entity, List<string> warnings, CancellationToken cancellationToken) =>
        await Repository.ExistsAsync(a => a.Id != entity.Id, cancellationToken)
            ? null
            : Result<Guid>.Conflict("id", "No se puede eliminar el único administrativo del sistema.");
}
