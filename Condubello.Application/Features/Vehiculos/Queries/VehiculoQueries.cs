namespace Condubello.Application.Features.Vehiculos;

public record GetVehiculosQuery : IRequest<Result<IReadOnlyList<VehiculoDto>>>;

public class GetVehiculosHandler(IRepository<Vehiculo> repository, IMapper mapper)
    : GetAllHandler<GetVehiculosQuery, Vehiculo, VehiculoDto>(repository, mapper);

public record GetVehiculoByIdQuery(Guid Id) : IRequest<Result<VehiculoDto>>, IByIdRequest;

public class GetVehiculoByIdHandler(IRepository<Vehiculo> repository, IMapper mapper)
    : GetByIdHandler<GetVehiculoByIdQuery, Vehiculo, VehiculoDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el vehículo.";
}
