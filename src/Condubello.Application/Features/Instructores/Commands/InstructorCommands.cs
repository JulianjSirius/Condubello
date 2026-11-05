using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Instructores;

[Autorizar(Rol.Administrativo)]
public record CreateInstructorCommand(CreateInstructorRequest Data) : IRequest<Result<InstructorDto>>, IPersonaCommand
{
    IPersonaRequest IPersonaCommand.Data => Data;
}

public class CreateInstructorHandler(
    IRepository<Instructor> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : CreatePersonaHandler<CreateInstructorCommand, Instructor, InstructorDto>(repository, validator, passwordHasher, unitOfWork, mapper)
{
    protected override Rol Rol => Rol.Instructor;
}

[Autorizar(Rol.Administrativo)]
public record UpdateInstructorCommand(Guid Id, UpdateInstructorRequest Data)
    : IRequest<Result<InstructorDto>>, IPersonaCommand, IByIdRequest
{
    IPersonaRequest IPersonaCommand.Data => Data;
}

public class UpdateInstructorHandler(
    IRepository<Instructor> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : UpdatePersonaHandler<UpdateInstructorCommand, Instructor, InstructorDto>(repository, validator, passwordHasher, unitOfWork, mapper)
{
    protected override Rol Rol => Rol.Instructor;
}

[Autorizar(Rol.Administrativo)]
public record DeleteInstructorCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteInstructorHandler(
    IRepository<Instructor> repository,
    IRepository<SesionPractica> sesiones,
    IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteInstructorCommand, Instructor>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el instructor.";

    protected override async Task<Result<Guid>?> ValidateAsync(
        Instructor entity, List<string> warnings, CancellationToken cancellationToken) =>
        await sesiones.ExistsAsync(s => s.InstructorId == entity.Id, cancellationToken)
            ? Result<Guid>.Conflict("id", "No se puede eliminar el instructor porque tiene sesiones prácticas registradas.")
            : null;
}
