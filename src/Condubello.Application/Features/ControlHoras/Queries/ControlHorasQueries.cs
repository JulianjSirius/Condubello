using System.Linq.Expressions;

namespace Condubello.Application.Features.ControlHoras;

public record GetControlHorasQuery : IRequest<Result<IReadOnlyList<ControlHorasDto>>>;

public class GetControlHorasHandler(IRepository<ControlHorasEstudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetControlHorasQuery, ControlHorasEstudiante, ControlHorasDto>(repository, mapper)
{
    protected override Expression<Func<ControlHorasEstudiante, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return c => c.EstudianteId == yo;
    }
}

public record GetControlHorasByIdQuery(Guid Id) : IRequest<Result<ControlHorasDto>>, IByIdRequest;

public class GetControlHorasByIdHandler(IRepository<ControlHorasEstudiante> repository, ICurrentUser currentUser, IMapper mapper)
    : GetByIdHandler<GetControlHorasByIdQuery, ControlHorasEstudiante, ControlHorasDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el control de horas.";

    protected override Task<bool> PuedeVerAsync(ControlHorasEstudiante entity, CancellationToken cancellationToken) =>
        Task.FromResult(!currentUser.EsEstudiante || entity.EstudianteId == currentUser.Id);
}
