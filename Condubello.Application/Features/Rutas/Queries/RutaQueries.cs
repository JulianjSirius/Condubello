using System.Linq.Expressions;

namespace Condubello.Application.Features.Rutas;

public record GetRutasQuery : IRequest<Result<IReadOnlyList<RutaDto>>>;

public class GetRutasHandler(IRepository<Ruta> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetRutasQuery, Ruta, RutaDto>(repository, mapper)
{
    /// <summary>Los instructores solo ven las rutas activas.</summary>
    protected override Expression<Func<Ruta, bool>>? Filtro() =>
        currentUser.EsInstructor ? r => r.Activa : null;
}

public record GetRutaByIdQuery(Guid Id) : IRequest<Result<RutaDto>>, IByIdRequest;

public class GetRutaByIdHandler(IRepository<Ruta> repository, ICurrentUser currentUser, IMapper mapper)
    : GetByIdHandler<GetRutaByIdQuery, Ruta, RutaDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró la ruta.";

    protected override Task<bool> PuedeVerAsync(Ruta entity, CancellationToken cancellationToken) =>
        Task.FromResult(!currentUser.EsInstructor || entity.Activa);
}
