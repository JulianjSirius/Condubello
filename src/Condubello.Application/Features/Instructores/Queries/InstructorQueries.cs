using System.Linq.Expressions;

namespace Condubello.Application.Features.Instructores;

public record GetInstructoresQuery : IRequest<Result<IReadOnlyList<InstructorDto>>>;

/// <summary>El estudiante solo ve los instructores con los que tiene sesiones.</summary>
public class GetInstructoresHandler(IRepository<Instructor> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetInstructoresQuery, Instructor, InstructorDto>(repository, mapper)
{
    protected override Expression<Func<Instructor, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return i => i.Sesiones.Any(s => s.EstudianteId == yo);
    }
}

public record GetInstructorByIdQuery(Guid Id) : IRequest<Result<InstructorDto>>, IByIdRequest;

public class GetInstructorByIdHandler(
    IRepository<Instructor> repository,
    IRepository<SesionPractica> sesiones,
    ICurrentUser currentUser,
    IMapper mapper)
    : GetByIdHandler<GetInstructorByIdQuery, Instructor, InstructorDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el instructor.";

    protected override async Task<bool> PuedeVerAsync(Instructor entity, CancellationToken cancellationToken)
    {
        if (!currentUser.EsEstudiante)
            return true;

        var yo = currentUser.Id;
        return await sesiones.ExistsAsync(s => s.InstructorId == entity.Id && s.EstudianteId == yo, cancellationToken);
    }
}
