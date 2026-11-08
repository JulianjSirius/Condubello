using System.Linq.Expressions;

namespace Condubello.Application.Features.Telemetria;

public record GetTelemetriaPruebasQuery : IRequest<Result<IReadOnlyList<TelemetriaPruebaDto>>>;

public class GetTelemetriaPruebasHandler(IRepository<TelemetriaPrueba> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetTelemetriaPruebasQuery, TelemetriaPrueba, TelemetriaPruebaDto>(repository, mapper)
{
    protected override Expression<Func<TelemetriaPrueba, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return t => t.Sesion.EstudianteId == yo;
    }
}

public record GetTelemetriaPruebaByIdQuery(Guid Id) : IRequest<Result<TelemetriaPruebaDto>>, IByIdRequest;

public class GetTelemetriaPruebaByIdHandler(
    IRepository<TelemetriaPrueba> repository,
    IRepository<SesionPractica> sesiones,
    ICurrentUser currentUser,
    IMapper mapper)
    : GetByIdHandler<GetTelemetriaPruebaByIdQuery, TelemetriaPrueba, TelemetriaPruebaDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el registro de telemetría.";

    protected override async Task<bool> PuedeVerAsync(TelemetriaPrueba entity, CancellationToken cancellationToken)
    {
        if (!currentUser.EsEstudiante)
            return true;

        var yo = currentUser.Id;
        return await sesiones.ExistsAsync(s => s.Id == entity.SesionId && s.EstudianteId == yo, cancellationToken);
    }
}
