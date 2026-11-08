using System.Globalization;
using System.Text.Json;
using Condubello.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Condubello.Infrastructure.Routing;

/// <summary>Cliente HTTP de OSRM (https://project-osrm.org): /route para trazar y /match para ajustar puntos GPS a la vía.</summary>
public class OsrmRoutingProvider(HttpClient http, IOptions<OsrmSettings> options, ILogger<OsrmRoutingProvider> logger) : IRoutingProvider
{
    private readonly OsrmSettings _settings = options.Value;

    public async Task<RutaCalculada?> CalcularRutaAsync(IReadOnlyList<(double Lon, double Lat)> puntos, CancellationToken cancellationToken = default)
    {
        if (puntos.Count < 2)
            return null;

        var url = $"{Base()}/route/v1/driving/{Coordenadas(puntos)}?overview=full&geometries=geojson&continue_straight=false";
        using var doc = await GetJsonAsync(url, cancellationToken);
        if (doc is null || !EsOk(doc.RootElement) || !doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
            return null;

        var ruta = routes[0];
        var coords = ruta.GetProperty("geometry").GetProperty("coordinates").EnumerateArray()
            .Select(c => (c[0].GetDouble(), c[1].GetDouble()))
            .ToList();
        return new RutaCalculada(coords, ruta.GetProperty("distance").GetDouble(), ruta.GetProperty("duration").GetDouble());
    }

    public async Task<IReadOnlyList<GpsPunto>?> AjustarPuntosAsync(IReadOnlyList<GpsPunto> puntos, CancellationToken cancellationToken = default)
    {
        if (puntos.Count < 2)
            return null;

        var radios = string.Join(';', puntos.Select(_ => _settings.RadioAjusteMetros));
        var coords = Coordenadas(puntos.Select(p => (p.Longitud, p.Latitud)).ToList());
        var url = $"{Base()}/match/v1/driving/{coords}?overview=false&gaps=ignore&radiuses={radios}";
        using var doc = await GetJsonAsync(url, cancellationToken);
        if (doc is null || !EsOk(doc.RootElement) || !doc.RootElement.TryGetProperty("tracepoints", out var trace) || trace.GetArrayLength() != puntos.Count)
            return null;

        var resultado = new List<GpsPunto>(puntos.Count);
        var i = 0;
        foreach (var t in trace.EnumerateArray())
        {
            var original = puntos[i++];
            resultado.Add(t.ValueKind == JsonValueKind.Object && t.TryGetProperty("location", out var loc)
                ? original with { Longitud = loc[0].GetDouble(), Latitud = loc[1].GetDouble() }
                : original);
        }
        return resultado;
    }

    private string Base() => _settings.BaseUrl.TrimEnd('/');

    private static string Coordenadas(IEnumerable<(double Lon, double Lat)> puntos) =>
        string.Join(';', puntos.Select(p => string.Create(CultureInfo.InvariantCulture, $"{p.Lon:0.######},{p.Lat:0.######}")));

    private static bool EsOk(JsonElement raiz) =>
        raiz.TryGetProperty("code", out var code) && code.GetString() == "Ok";

    private async Task<JsonDocument?> GetJsonAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var respuesta = await http.GetAsync(url, cancellationToken);
            var texto = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            if (!respuesta.IsSuccessStatusCode)
                logger.LogWarning("OSRM respondió {Status}: {Cuerpo}", (int)respuesta.StatusCode, texto);
            return JsonDocument.Parse(texto);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "No se pudo consultar OSRM en {BaseUrl}.", _settings.BaseUrl);
            return null;
        }
    }
}
