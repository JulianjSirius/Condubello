using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

internal static class GpsReglas
{
    /// <summary>Distancia máxima (metros) a la ruta autorizada antes de considerar una desviación.</summary>
    public const double ToleranciaMetros = 150;

    private const double MetrosPorGrado = 111_320;

    public static string ConstruirLineString(IReadOnlyList<GpsPunto> puntos)
    {
        var coords = puntos.Select(p => new[] { p.Longitud, p.Latitud });
        return JsonSerializer.Serialize(new { type = "LineString", coordinates = coords });
    }

    /// <summary>Extrae los vértices [lon, lat] de un GeoJSON (LineString, Feature o FeatureCollection).</summary>
    public static List<(double Lon, double Lat)> VerticesRuta(string? geoJson)
    {
        var vertices = new List<(double, double)>();
        if (string.IsNullOrWhiteSpace(geoJson))
            return vertices;

        using var doc = JsonDocument.Parse(geoJson);
        Recolectar(doc.RootElement, vertices);
        return vertices;
    }

    private static void Recolectar(JsonElement el, List<(double, double)> vertices)
    {
        if (el.ValueKind == JsonValueKind.Array)
        {
            if (el.GetArrayLength() >= 2 && el[0].ValueKind == JsonValueKind.Number && el[1].ValueKind == JsonValueKind.Number)
                vertices.Add((el[0].GetDouble(), el[1].GetDouble()));
            else
                foreach (var hijo in el.EnumerateArray())
                    Recolectar(hijo, vertices);
        }
        else if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
                if (prop.Name is "coordinates" or "geometry" or "features" or "geometries")
                    Recolectar(prop.Value, vertices);
        }
    }

    public static bool HayDesviacion(IReadOnlyList<GpsPunto> puntos, List<(double Lon, double Lat)> ruta)
    {
        if (ruta.Count == 0)
            return false;

        return puntos.Any(p => DistanciaARutaMetros(p, ruta) > ToleranciaMetros);
    }

    private static double DistanciaARutaMetros(GpsPunto p, List<(double Lon, double Lat)> ruta)
    {
        if (ruta.Count == 1)
            return Distancia(p.Longitud, p.Latitud, ruta[0].Lon, ruta[0].Lat, p.Latitud);

        var min = double.MaxValue;
        for (var i = 0; i < ruta.Count - 1; i++)
            min = Math.Min(min, DistanciaASegmento(p, ruta[i], ruta[i + 1]));
        return min;
    }

    // Aproximación equirectangular: suficiente para distancias cortas.
    private static double DistanciaASegmento(GpsPunto p, (double Lon, double Lat) a, (double Lon, double Lat) b)
    {
        var k = Math.Cos(p.Latitud * Math.PI / 180);
        double X(double lon) => (lon - p.Longitud) * k * MetrosPorGrado;
        double Y(double lat) => (lat - p.Latitud) * MetrosPorGrado;

        var (ax, ay, bx, by) = (X(a.Lon), Y(a.Lat), X(b.Lon), Y(b.Lat));
        var (dx, dy) = (bx - ax, by - ay);
        var largo2 = dx * dx + dy * dy;
        var t = largo2 == 0 ? 0 : Math.Clamp(-(ax * dx + ay * dy) / largo2, 0, 1);
        return Math.Sqrt(Math.Pow(ax + t * dx, 2) + Math.Pow(ay + t * dy, 2));
    }

    private static double Distancia(double lon1, double lat1, double lon2, double lat2, double latRef)
    {
        var k = Math.Cos(latRef * Math.PI / 180);
        var dx = (lon2 - lon1) * k * MetrosPorGrado;
        var dy = (lat2 - lat1) * MetrosPorGrado;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

[Autorizar(Rol.Administrativo)]
public record SincronizarGpsCommand(SincronizarGpsRequest Data) : IRequest<Result<SincronizacionGpsDto>>;

public class SincronizarGpsHandler(
    IRepository<SesionPractica> sesiones,
    IRepository<TelemetriaPrueba> telemetrias,
    IRepository<Ruta> rutas,
    IGpsProvider gps,
    IUnitOfWork unitOfWork) : IRequestHandler<SincronizarGpsCommand, Result<SincronizacionGpsDto>>
{
    public async Task<Result<SincronizacionGpsDto>> Handle(SincronizarGpsCommand request, CancellationToken cancellationToken)
    {
        var sesionId = request.Data.SesionId;

        IReadOnlyList<SesionPractica> objetivo;
        if (sesionId is { } id)
        {
            var sesion = await sesiones.GetByIdAsync(id, cancellationToken);
            if (sesion is null)
                return Result<SincronizacionGpsDto>.NotFound($"No se encontró la sesión práctica con id {id}.");
            objetivo = [sesion];
        }
        else
        {
            objetivo = await sesiones.ListAsync(
                s => s.EstadoPrueba == EstadoPrueba.EnCurso || s.EstadoPrueba == EstadoPrueba.Completada,
                cancellationToken);
        }

        int sincronizadas = 0, conAlerta = 0, sinDatos = 0;
        var cacheRutas = new Dictionary<Guid, List<(double Lon, double Lat)>>();

        foreach (var sesion in objetivo)
        {
            var puntos = await gps.ObtenerPuntosAsync(sesion.Id, sesion.FechaHoraInicio, sesion.FechaHoraFin, cancellationToken);
            if (puntos.Count == 0)
            {
                sinDatos++;
                continue;
            }

            var ruta = new List<(double Lon, double Lat)>();
            if (sesion.RutaId is { } rutaId && !cacheRutas.TryGetValue(rutaId, out ruta!))
            {
                var entidad = await rutas.GetByIdAsync(rutaId, cancellationToken);
                ruta = GpsReglas.VerticesRuta(entidad?.GeoJson);
                cacheRutas[rutaId] = ruta;
            }
            ruta ??= [];

            var desviacion = GpsReglas.HayDesviacion(puntos, ruta);

            var telemetria = await telemetrias.FirstOrDefaultAsync(t => t.SesionId == sesion.Id, cancellationToken);
            var esNueva = telemetria is null;
            telemetria ??= new TelemetriaPrueba { SesionId = sesion.Id };

            telemetria.RutaGps = GpsReglas.ConstruirLineString(puntos);
            telemetria.AlertaDesviacion = desviacion;
            telemetria.ResumenTelemetria ??= JsonSerializer.Serialize(new
            {
                fuente = "gps-sincronizado",
                puntos = puntos.Count,
                sincronizadoEn = DateTime.UtcNow
            });

            if (esNueva)
                await telemetrias.AddAsync(telemetria, cancellationToken);
            else
                telemetrias.Update(telemetria);

            sincronizadas++;
            if (desviacion)
                conAlerta++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var warnings = conAlerta > 0
            ? new[] { $"{conAlerta} sesión(es) con desviación de la ruta autorizada." }
            : null;
        return Result<SincronizacionGpsDto>.Success(new SincronizacionGpsDto(sincronizadas, conAlerta, sinDatos), warnings);
    }
}
