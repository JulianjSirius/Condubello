using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

internal static class TelemetriaReglas
{
    private static readonly string[] TiposGeoJson =
    [
        "Point", "MultiPoint", "LineString", "MultiLineString", "Polygon",
        "MultiPolygon", "GeometryCollection", "Feature", "FeatureCollection"
    ];

    public static List<ApiError> Validar(TelemetriaPruebaRequestBase data)
    {
        var errores = new List<ApiError>();

        if (data.ResumenTelemetria is { } resumen && resumen.ValueKind is not (JsonValueKind.Object or JsonValueKind.Null))
            errores.Add(new ApiError("resumenTelemetria", "El resumen de telemetría debe ser un objeto JSON."));

        if (ValidarRutaGps(data.RutaGps) is { } errorRuta)
            errores.Add(errorRuta);

        return errores;
    }

    public static ApiError? ValidarRutaGps(JsonElement? rutaGps) =>
        rutaGps is { } ruta && ruta.ValueKind != JsonValueKind.Null && !EsGeoJsonValido(ruta)
            ? new ApiError("rutaGps", "La ruta GPS debe ser un GeoJSON válido (objeto con una propiedad \"type\" reconocida).")
            : null;

    public static List<string> Advertencias(TelemetriaPrueba telemetria) =>
        telemetria.AlertaDesviacion
            ? ["Se detectó una desviación de la ruta durante la prueba."]
            : [];

    public static void Aplicar(TelemetriaPrueba entity, TelemetriaPruebaRequestBase data)
    {
        entity.ResumenTelemetria = JsonHelper.ToJson(data.ResumenTelemetria);
        entity.RutaGps = JsonHelper.ToJson(data.RutaGps);
        entity.AlertaDesviacion = data.AlertaDesviacion;
    }

    private static bool EsGeoJsonValido(JsonElement ruta) =>
        ruta.ValueKind == JsonValueKind.Object &&
        ruta.TryGetProperty("type", out var tipo) &&
        tipo.ValueKind == JsonValueKind.String &&
        TiposGeoJson.Contains(tipo.GetString());
}

[Autorizar(Rol.Administrativo)]
public record CreateTelemetriaPruebaCommand(CreateTelemetriaPruebaRequest Data) : IRequest<Result<TelemetriaPruebaDto>>;

public class CreateTelemetriaPruebaHandler(
    IRoutingProvider routing,
    IRepository<TelemetriaPrueba> repository,
    IRepository<SesionPractica> sesiones,
    IRepository<Ruta> rutas,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateTelemetriaPruebaCommand, Result<TelemetriaPruebaDto>>
{
    public async Task<Result<TelemetriaPruebaDto>> Handle(CreateTelemetriaPruebaCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var sesionId = data.SesionId!.Value;

        if (!await sesiones.ExistsAsync(s => s.Id == sesionId, cancellationToken))
            return Result<TelemetriaPruebaDto>.NotFound($"No se encontró la sesión práctica con id {sesionId}.");

        var errores = TelemetriaReglas.Validar(data);
        if (errores.Count > 0)
            return Result<TelemetriaPruebaDto>.Validation(errores);

        var entity = new TelemetriaPrueba { SesionId = sesionId };
        TelemetriaReglas.Aplicar(entity, data);
        var avisos = TelemetriaReglas.Advertencias(entity);
        if (await GpsReglas.AjustarRutaGpsAsync(entity.RutaGps, routing, cancellationToken) is { } ajustada)
            entity.RutaGps = ajustada;
        else if (entity.RutaGps is not null)
            avisos.Add("La ruta GPS se guardó sin ajustar a las calles (OSRM no respondió o no es un LineString).");
        if (await GpsReglas.DesviacionDeSesionAsync(sesionId, entity.RutaGps, sesiones, rutas, cancellationToken) is { } desviacion)
            entity.AlertaDesviacion = desviacion;

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TelemetriaPruebaDto>.Success(mapper.Map<TelemetriaPruebaDto>(entity), avisos);
    }
}

[Autorizar(Rol.Administrativo)]
public record UpdateTelemetriaPruebaCommand(Guid Id, UpdateTelemetriaPruebaRequest Data) : IRequest<Result<TelemetriaPruebaDto>>;

public class UpdateTelemetriaPruebaHandler(
    IRoutingProvider routing,
    IRepository<TelemetriaPrueba> repository,
    IRepository<SesionPractica> sesiones,
    IRepository<Ruta> rutas,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateTelemetriaPruebaCommand, Result<TelemetriaPruebaDto>>
{
    public async Task<Result<TelemetriaPruebaDto>> Handle(UpdateTelemetriaPruebaCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<TelemetriaPruebaDto>.NotFound($"No se encontró el registro de telemetría con id {request.Id}.");

        var errores = TelemetriaReglas.Validar(request.Data);
        if (errores.Count > 0)
            return Result<TelemetriaPruebaDto>.Validation(errores);

        TelemetriaReglas.Aplicar(entity, request.Data);
        var avisos = TelemetriaReglas.Advertencias(entity);
        if (await GpsReglas.AjustarRutaGpsAsync(entity.RutaGps, routing, cancellationToken) is { } ajustada)
            entity.RutaGps = ajustada;
        else if (entity.RutaGps is not null)
            avisos.Add("La ruta GPS se guardó sin ajustar a las calles (OSRM no respondió o no es un LineString).");
        if (await GpsReglas.DesviacionDeSesionAsync(entity.SesionId, entity.RutaGps, sesiones, rutas, cancellationToken) is { } desviacion)
            entity.AlertaDesviacion = desviacion;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TelemetriaPruebaDto>.Success(mapper.Map<TelemetriaPruebaDto>(entity), avisos);
    }
}

/// <summary>Actualiza solo la ruta GPS. El instructor solo puede actualizar la telemetría de sus sesiones.</summary>
[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record ActualizarRutaGpsCommand(Guid Id, ActualizarRutaGpsRequest Data) : IRequest<Result<TelemetriaPruebaDto>>;

public class ActualizarRutaGpsHandler(
    IRoutingProvider routing,
    IRepository<TelemetriaPrueba> repository,
    IRepository<SesionPractica> sesiones,
    IRepository<Ruta> rutas,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ActualizarRutaGpsCommand, Result<TelemetriaPruebaDto>>
{
    public async Task<Result<TelemetriaPruebaDto>> Handle(ActualizarRutaGpsCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<TelemetriaPruebaDto>.NotFound($"No se encontró el registro de telemetría con id {request.Id}.");

        if (currentUser.EsInstructor)
        {
            var yo = currentUser.Id;
            if (!await sesiones.ExistsAsync(s => s.Id == entity.SesionId && s.InstructorId == yo, cancellationToken))
                return Result<TelemetriaPruebaDto>.Forbidden("Solo puede modificar la telemetría de sus propias sesiones.");
        }

        if (TelemetriaReglas.ValidarRutaGps(request.Data.RutaGps) is { } error)
            return Result<TelemetriaPruebaDto>.Validation([error]);

        entity.RutaGps = JsonHelper.ToJson(request.Data.RutaGps);
        var avisos = TelemetriaReglas.Advertencias(entity);
        if (await GpsReglas.AjustarRutaGpsAsync(entity.RutaGps, routing, cancellationToken) is { } ajustada)
            entity.RutaGps = ajustada;
        else if (entity.RutaGps is not null)
            avisos.Add("La ruta GPS se guardó sin ajustar a las calles (OSRM no respondió o no es un LineString).");
        entity.AlertaDesviacion = await GpsReglas.DesviacionDeSesionAsync(entity.SesionId, entity.RutaGps, sesiones, rutas, cancellationToken)
            ?? entity.AlertaDesviacion;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TelemetriaPruebaDto>.Success(mapper.Map<TelemetriaPruebaDto>(entity), avisos);
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteTelemetriaPruebaCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteTelemetriaPruebaHandler(IRepository<TelemetriaPrueba> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteTelemetriaPruebaCommand, TelemetriaPrueba>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el registro de telemetría.";
}
