namespace Condubello.Application.Common.Interfaces;

/// <summary>Ruta calculada sobre la red vial: vértices [lon, lat] y totales.</summary>
public record RutaCalculada(IReadOnlyList<(double Lon, double Lat)> Coordenadas, double DistanciaMetros, double DuracionSegundos);

/// <summary>Motor de ruteo sobre calles reales (OSRM autohospedado). Devuelve null si el servicio no está disponible o no hay resultado.</summary>
public interface IRoutingProvider
{
    /// <summary>Ruta por calles que pasa por los puntos dados, en orden. Mínimo 2 puntos.</summary>
    Task<RutaCalculada?> CalcularRutaAsync(IReadOnlyList<(double Lon, double Lat)> puntos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajusta cada punto GPS a la calle más cercana. Devuelve una lista del mismo largo y orden que la entrada;
    /// los puntos que no se pudieron ajustar se devuelven sin cambios.
    /// </summary>
    Task<IReadOnlyList<GpsPunto>?> AjustarPuntosAsync(IReadOnlyList<GpsPunto> puntos, CancellationToken cancellationToken = default);
}
