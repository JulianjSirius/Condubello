namespace Condubello.Application.Features.Rutas;

public record GetRutasQuery : IRequest<Result<IReadOnlyList<RutaDto>>>;

public class GetRutasHandler(IRepository<Ruta> repository, IMapper mapper)
    : GetAllHandler<GetRutasQuery, Ruta, RutaDto>(repository, mapper);

public record GetRutaByIdQuery(Guid Id) : IRequest<Result<RutaDto>>, IByIdRequest;

public class GetRutaByIdHandler(IRepository<Ruta> repository, IMapper mapper)
    : GetByIdHandler<GetRutaByIdQuery, Ruta, RutaDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró la ruta.";
}
