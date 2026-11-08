using Condubello.Application.Features.Telemetria;

namespace Condubello.Application.Features.Rutas;

internal static class RutaReglas
{
    public static string Conflicto(string nombre) => $"Ya existe una ruta con el nombre {nombre}.";

    public static ApiError? Validar(RutaRequestBase data) =>
        TelemetriaReglas.ValidarRutaGps(data.GeoJson) is { } error
            ? new ApiError("geoJson", "El GeoJSON de la ruta no es válido (objeto con una propiedad \"type\" reconocida).")
            : null;

    public static void Aplicar(Ruta entity, RutaRequestBase data)
    {
        entity.Nombre = data.Nombre.Trim();
        entity.Descripcion = string.IsNullOrWhiteSpace(data.Descripcion) ? null : data.Descripcion.Trim();
        entity.GeoJson = JsonHelper.ToJson(data.GeoJson);
        entity.Activa = data.Activa;
    }
}

[Autorizar(Rol.Administrativo)]
public record CreateRutaCommand(CreateRutaRequest Data) : IRequest<Result<RutaDto>>;

public class CreateRutaHandler(IRepository<Ruta> repository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateRutaCommand, Result<RutaDto>>
{
    public async Task<Result<RutaDto>> Handle(CreateRutaCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        if (RutaReglas.Validar(data) is { } error)
            return Result<RutaDto>.Validation([error]);

        var nombre = data.Nombre.Trim();
        if (await repository.ExistsAsync(r => r.Nombre == nombre, cancellationToken))
            return Result<RutaDto>.Conflict("nombre", RutaReglas.Conflicto(nombre));

        var entity = new Ruta();
        RutaReglas.Aplicar(entity, data);

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RutaDto>.Success(mapper.Map<RutaDto>(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record UpdateRutaCommand(Guid Id, UpdateRutaRequest Data) : IRequest<Result<RutaDto>>;

public class UpdateRutaHandler(IRepository<Ruta> repository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateRutaCommand, Result<RutaDto>>
{
    public async Task<Result<RutaDto>> Handle(UpdateRutaCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<RutaDto>.NotFound($"No se encontró la ruta con id {request.Id}.");

        var data = request.Data;
        if (RutaReglas.Validar(data) is { } error)
            return Result<RutaDto>.Validation([error]);

        var nombre = data.Nombre.Trim();
        if (await repository.ExistsAsync(r => r.Nombre == nombre && r.Id != entity.Id, cancellationToken))
            return Result<RutaDto>.Conflict("nombre", RutaReglas.Conflicto(nombre));

        RutaReglas.Aplicar(entity, data);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RutaDto>.Success(mapper.Map<RutaDto>(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteRutaCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteRutaHandler(IRepository<Ruta> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteRutaCommand, Ruta>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró la ruta.";
}
