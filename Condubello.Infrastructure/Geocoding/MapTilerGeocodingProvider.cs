using System.Globalization;
using System.Text.Json;
using Condubello.Application.Common.Interfaces;
using Condubello.Domain.Entities;
using Condubello.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Condubello.Infrastructure.Geocoding;

/// <summary>
/// Geocodificación con la API de MapTiler. Cada resultado se guarda en la tabla geocodificacion_cache
/// y se reutiliza antes de llamar al proveedor, para reducir las consultas.
/// </summary>
public class MapTilerGeocodingProvider(
    HttpClient http,
    ApplicationDbContext db,
    IOptions<MapTilerSettings> options,
    ILogger<MapTilerGeocodingProvider> logger) : IGeocodingProvider
{
    private const string Base = "https://api.maptiler.com/geocoding";
    private readonly string _apiKey = options.Value.ApiKey?.Trim() ?? string.Empty;

    public bool Configurado => _apiKey.Length > 0;

    public Task<DireccionGeocodificada?> InversaAsync(double latitud, double longitud, CancellationToken cancellationToken = default)
    {
        var clave = string.Create(CultureInfo.InvariantCulture, $"r:{latitud:0.0000},{longitud:0.0000}");
        // MapTiler recibe las coordenadas como longitud,latitud.
        var url = string.Create(CultureInfo.InvariantCulture, $"{Base}/{longitud},{latitud}.json?language=es&limit=1");
        return ConCacheAsync(clave, url, cancellationToken);
    }

    public Task<DireccionGeocodificada?> BuscarAsync(string texto, CancellationToken cancellationToken = default)
    {
        var normalizado = string.Join(' ', texto.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var clave = $"s:{normalizado}";
        var url = $"{Base}/{Uri.EscapeDataString(normalizado)}.json?country=co&language=es&limit=1";
        return ConCacheAsync(clave, url, cancellationToken);
    }

    private async Task<DireccionGeocodificada?> ConCacheAsync(string clave, string url, CancellationToken cancellationToken)
    {
        if (clave.Length > 300 || !Configurado)
            return null;

        var guardado = await db.GeocodificacionCache.AsNoTracking().FirstOrDefaultAsync(c => c.Clave == clave, cancellationToken);
        if (guardado is not null)
            return JsonSerializer.Deserialize<DireccionGeocodificada>(guardado.Resultado);

        var direccion = await ConsultarAsync(url, cancellationToken);
        if (direccion is null)
            return null;

        try
        {
            db.GeocodificacionCache.Add(new GeocodificacionCache { Clave = clave, Resultado = JsonSerializer.Serialize(direccion) });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Otra petición la guardó al mismo tiempo: no pasa nada.
            db.ChangeTracker.Clear();
        }
        return direccion;
    }

    private async Task<DireccionGeocodificada?> ConsultarAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            // La clave va en la URL: por eso System.Net.Http.HttpClient está en Warning en appsettings (no se registra la URL).
            using var respuesta = await http.GetAsync($"{url}&key={Uri.EscapeDataString(_apiKey)}", cancellationToken);
            if (!respuesta.IsSuccessStatusCode)
            {
                logger.LogWarning("MapTiler respondió {Status}.", (int)respuesta.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(cancellationToken));
            if (!doc.RootElement.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
                return null;

            var f = features[0];
            if (!f.TryGetProperty("place_name", out var nombre) || nombre.GetString() is not { Length: > 0 } texto)
                return null;

            // "center" es [longitud, latitud].
            var centro = f.GetProperty("center");
            return new DireccionGeocodificada(texto, centro[1].GetDouble(), centro[0].GetDouble());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            logger.LogWarning("No se pudo consultar MapTiler: {Tipo}.", ex.GetType().Name);
            return null;
        }
    }
}
