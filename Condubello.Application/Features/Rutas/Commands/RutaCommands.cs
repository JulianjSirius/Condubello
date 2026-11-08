using System.Text.Json;
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

/// <summary>Reemplaza los puntos de la ruta por el trazado real sobre las calles (OSRM).</summary>
[Autorizar(Rol.Administrativo)]
public record AjustarRutaACallesCommand(Guid Id) : IRequest<Result<RutaDto>>;

public class AjustarRutaACallesHandler(IRepository<Ruta> repository, IRoutingProvider routing, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<AjustarRutaACallesCommand, Result<RutaDto>>
{
    private const int MaxPuntos = 100;

    public async Task<Result<RutaDto>> Handle(AjustarRutaACallesCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<RutaDto>.NotFound($"No se encontró la ruta con id {request.Id}.");

        var puntos = GpsReglas.VerticesRuta(entity.GeoJson);
        if (puntos.Count < 2)
            return Result<RutaDto>.Invalid("geoJson", "La ruta necesita al menos 2 puntos para calcular el trazado por calles.");
        if (puntos.Count > MaxPuntos)
            return Result<RutaDto>.Invalid("geoJson", $"La ruta tiene {puntos.Count} puntos (máximo {MaxPuntos}); parece que ya sigue las calles.");

        var calculada = await routing.CalcularRutaAsync(puntos, cancellationToken);
        if (calculada is null)
            return Result<RutaDto>.Invalid("geoJson", "No se pudo calcular el trazado: el servicio de rutas (OSRM) no responde o no hay camino entre los puntos.");

        entity.GeoJson = JsonSerializer.Serialize(new
        {
            type = "LineString",
            coordinates = calculada.Coordenadas.Select(c => new[] { c.Lon, c.Lat })
        });

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RutaDto>.Success(
            mapper.Map<RutaDto>(entity),
            [$"Ruta ajustada a las calles: {calculada.DistanciaMetros / 1000:0.0} km, ~{calculada.DuracionSegundos / 60:0} min en auto."]);
    }
}

public record CalcularTrazadoRequest(double OrigenLat, double OrigenLon, double DestinoLat, double DestinoLon);

public record TrazadoDto(JsonElement GeoJson, double DistanciaKm, double DuracionMin);

/// <summary>Calcula (sin guardar) el trazado por calles entre un punto A y un punto B.</summary>
[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record CalcularTrazadoCommand(CalcularTrazadoRequest Data) : IRequest<Result<TrazadoDto>>;

public class CalcularTrazadoHandler(IRoutingProvider routing)
    : IRequestHandler<CalcularTrazadoCommand, Result<TrazadoDto>>
{
    public async Task<Result<TrazadoDto>> Handle(CalcularTrazadoCommand request, CancellationToken cancellationToken)
    {
        var d = request.Data;
        var calculada = await routing.CalcularRutaAsync([(d.OrigenLon, d.OrigenLat), (d.DestinoLon, d.DestinoLat)], cancellationToken);
        if (calculada is null)
            return Result<TrazadoDto>.Invalid("geoJson", "No se pudo calcular el trazado: el servicio de rutas (OSRM) no responde o no hay camino entre A y B.");

        var geo = JsonSerializer.SerializeToElement(new
        {
            type = "LineString",
            coordinates = calculada.Coordenadas.Select(c => new[] { c.Lon, c.Lat })
        });
        return Result<TrazadoDto>.Success(new TrazadoDto(geo, Math.Round(calculada.DistanciaMetros / 1000, 1), Math.Round(calculada.DuracionSegundos / 60)));
    }
}
