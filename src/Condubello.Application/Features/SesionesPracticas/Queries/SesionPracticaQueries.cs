using System.Linq.Expressions;

namespace Condubello.Application.Features.SesionesPracticas;

public record GetSesionesPracticasQuery : IRequest<Result<IReadOnlyList<SesionPracticaDto>>>;

public class GetSesionesPracticasHandler(IRepository<SesionPractica> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetSesionesPracticasQuery, SesionPractica, SesionPracticaDto>(repository, mapper)
{
    protected override Expression<Func<SesionPractica, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return s => s.EstudianteId == yo;
    }
}

public record GetSesionPracticaByIdQuery(Guid Id) : IRequest<Result<SesionPracticaDto>>, IByIdRequest;

public class GetSesionPracticaByIdHandler(IRepository<SesionPractica> repository, ICurrentUser currentUser, IMapper mapper)
    : GetByIdHandler<GetSesionPracticaByIdQuery, SesionPractica, SesionPracticaDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró la sesión práctica.";

    protected override Task<bool> PuedeVerAsync(SesionPractica entity, CancellationToken cancellationToken) =>
        Task.FromResult(!currentUser.EsEstudiante || entity.EstudianteId == currentUser.Id);
}
