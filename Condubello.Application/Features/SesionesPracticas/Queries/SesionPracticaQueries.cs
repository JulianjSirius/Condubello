using System.Linq.Expressions;

namespace Condubello.Application.Features.SesionesPracticas;

public record GetSesionesPracticasQuery : IRequest<Result<IReadOnlyList<SesionPracticaDto>>>;

public class GetSesionesPracticasHandler(IRepository<SesionPractica> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetSesionesPracticasQuery, SesionPractica, SesionPracticaDto>(repository, mapper)
{
    protected override Expression<Func<SesionPractica, bool>>? Filtro()
    {
        var yo = currentUser.Id;
        if (currentUser.EsEstudiante)
            return s => s.EstudianteId == yo;
        if (currentUser.EsInstructor)
            return s => s.InstructorId == yo;
        return null;
    }
}

public record GetSesionPracticaByIdQuery(Guid Id) : IRequest<Result<SesionPracticaDto>>, IByIdRequest;

public class GetSesionPracticaByIdHandler(IRepository<SesionPractica> repository, ICurrentUser currentUser, IMapper mapper)
    : GetByIdHandler<GetSesionPracticaByIdQuery, SesionPractica, SesionPracticaDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró la sesión práctica.";

    protected override Task<bool> PuedeVerAsync(SesionPractica entity, CancellationToken cancellationToken) =>
        Task.FromResult(
            currentUser.EsEstudiante ? entity.EstudianteId == currentUser.Id
            : !currentUser.EsInstructor || entity.InstructorId == currentUser.Id);
}
