using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

internal static class GpsReglas
{
    /// <summary>Distancia máxima (metros) a la ruta autorizada antes de considerar una desviación.</summary>
    public const double ToleranciaMetros = 100;

    private const double MetrosPorGrado = 111_320;

    /// <summary>
    /// Si el GeoJSON es un LineString de hasta 100 puntos, lo ajusta a las calles con OSRM.
    /// Devuelve el GeoJSON ajustado, o null si no se pudo (sin cambios).
    /// </summary>
    public static async Task<string?> AjustarRutaGpsAsync(string? geoJson, IRoutingProvider routing, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(geoJson))
            return null;

        using var doc = JsonDocument.Parse(geoJson);
        if (!doc.RootElement.TryGetProperty("type", out var tipo) || tipo.GetString() != "LineString")
            return null;

        var puntos = VerticesRuta(geoJson).Select(v => new GpsPunto(v.Lat, v.Lon, default)).ToList();
        if (puntos.Count > 100)
            return geoJson; // demasiado largo para /match: se asume ya trazado sobre las calles
        if (puntos.Count < 2)
            return null;

        var ajustados = await routing.AjustarPuntosAsync(puntos, ct);
        return ajustados is null ? null : ConstruirLineString(ajustados);
    }

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

    /// <summary>
    /// Compara dos GeoJSON: true si algún vértice del recorrido está a más de <see cref="ToleranciaMetros"/>
    /// de la ruta autorizada. Devuelve null si no se puede evaluar (falta alguno de los dos).
    /// </summary>
    public static bool? HayDesviacion(string? recorridoGeoJson, string? rutaAutorizadaGeoJson)
    {
        var ruta = VerticesRuta(rutaAutorizadaGeoJson);
        var recorrido = VerticesRuta(recorridoGeoJson);
        if (ruta.Count == 0 || recorrido.Count == 0)
            return null;

        var puntos = recorrido.Select(v => new GpsPunto(v.Lat, v.Lon, default)).ToList();
        return HayDesviacion(puntos, ruta);
    }

    /// <summary>Calcula la alerta de desviación de una sesión a partir de su ruta asignada. Null si no se puede evaluar.</summary>
    public static async Task<bool?> DesviacionDeSesionAsync(
        Guid sesionId, string? recorridoGeoJson,
        IRepository<SesionPractica> sesiones, IRepository<Ruta> rutas, CancellationToken ct)
    {
        var sesion = await sesiones.GetByIdAsync(sesionId, ct);
        if (sesion?.RutaId is not { } rutaId)
            return null;

        var ruta = await rutas.GetByIdAsync(rutaId, ct);
        return HayDesviacion(recorridoGeoJson, ruta?.GeoJson);
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
