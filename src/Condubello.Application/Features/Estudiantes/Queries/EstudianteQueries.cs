using System.Linq.Expressions;

namespace Condubello.Application.Features.Estudiantes;

public record GetEstudiantesQuery : IRequest<Result<IReadOnlyList<EstudianteDto>>>;

public class GetEstudiantesHandler(IRepository<Estudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetEstudiantesQuery, Estudiante, EstudianteDto>(repository, mapper)
{
    protected override Expression<Func<Estudiante, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return e => e.Id == yo;
    }
}

public record GetEstudianteByIdQuery(Guid Id) : IRequest<Result<EstudianteDto>>, IByIdRequest;

public class GetEstudianteByIdHandler(IRepository<Estudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : GetByIdHandler<GetEstudianteByIdQuery, Estudiante, EstudianteDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el estudiante.";

    protected override Task<bool> PuedeVerAsync(Estudiante entity, CancellationToken cancellationToken) =>
        Task.FromResult(!currentUser.EsEstudiante || entity.Id == currentUser.Id);
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
