using Condubello.Application.Common.Interfaces;

namespace Condubello.Infrastructure.Gps;

/// <summary>
/// Proveedor GPS simulado (no hay un proveedor real configurado). Genera un recorrido determinista
/// por sesión para poder probar la sincronización; sustituir por la integración real del dispositivo.
/// </summary>
public class SimulatedGpsProvider : IGpsProvider
{
    private const double LatitudBase = 4.7110;   // Bogotá
    private const double LongitudBase = -74.0721;
    private const int Puntos = 20;

    public Task<IReadOnlyList<GpsPunto>> ObtenerPuntosAsync(
        Guid sesionId, DateTime inicio, DateTime? fin, CancellationToken cancellationToken = default)
    {
        var bytes = sesionId.ToByteArray();
        var rnd = new Random(BitConverter.ToInt32(bytes, 0));
        var duracion = (fin ?? inicio.AddHours(1)) - inicio;

        var lat = LatitudBase;
        var lon = LongitudBase;
        var lista = new List<GpsPunto>(Puntos);
        for (var i = 0; i < Puntos; i++)
        {
            lat += 0.0004 + (rnd.NextDouble() - 0.5) * 0.0002;
            lon += 0.0004 + (rnd.NextDouble() - 0.5) * 0.0002;
            lista.Add(new GpsPunto(lat, lon, inicio + duracion * i / (Puntos - 1)));
        }

        return Task.FromResult<IReadOnlyList<GpsPunto>>(lista);
    }
}
