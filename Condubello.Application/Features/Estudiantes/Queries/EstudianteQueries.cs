using System.Linq.Expressions;

namespace Condubello.Application.Features.Estudiantes;

public record GetEstudiantesQuery : IRequest<Result<IReadOnlyList<EstudianteDto>>>;

public class GetEstudiantesHandler(IRepository<Estudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetEstudiantesQuery, Estudiante, EstudianteDto>(repository, mapper)
{
    protected override Expression<Func<Estudiante, bool>>? Filtro()
    {
        var yo = currentUser.Id;
        if (currentUser.EsEstudiante)
            return e => e.Id == yo;
        if (currentUser.EsInstructor)
            return e => e.Sesiones.Any(s => s.InstructorId == yo);
        return null;
    }
}

public record GetEstudianteByIdQuery(Guid Id) : IRequest<Result<EstudianteDto>>, IByIdRequest;

public class GetEstudianteByIdHandler(
    IRepository<Estudiante> repository,
    IRepository<SesionPractica> sesiones,
    ICurrentUser currentUser,
    IMapper mapper)
    : GetByIdHandler<GetEstudianteByIdQuery, Estudiante, EstudianteDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el estudiante.";

    protected override async Task<bool> PuedeVerAsync(Estudiante entity, CancellationToken cancellationToken)
    {
        var yo = currentUser.Id;
        if (currentUser.EsEstudiante)
            return entity.Id == yo;
        if (currentUser.EsInstructor)
            return await sesiones.ExistsAsync(s => s.EstudianteId == entity.Id && s.InstructorId == yo, cancellationToken);
        return true;
    }
}

[Autorizar(Rol.Estudiante)]
public record GetMiPerfilEstudianteQuery : IRequest<Result<EstudianteDto>>;

public class GetMiPerfilEstudianteHandler(IRepository<Estudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetMiPerfilEstudianteQuery, Result<EstudianteDto>>
{
    public async Task<Result<EstudianteDto>> Handle(GetMiPerfilEstudianteQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(currentUser.Id, cancellationToken);
        return entity is null
            ? Result<EstudianteDto>.NotFound("No se encontró el estudiante autenticado.")
            : Result<EstudianteDto>.Success(mapper.Map<EstudianteDto>(entity));
    }
}
