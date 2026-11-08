namespace Condubello.Application.Common.Interfaces;

/// <summary>Punto GPS reportado por el proveedor del dispositivo del vehículo.</summary>
public record GpsPunto(double Latitud, double Longitud, DateTime Marca);

/// <summary>Origen de los datos GPS de las sesiones prácticas (dispositivo o proveedor externo).</summary>
public interface IGpsProvider
{
    Task<IReadOnlyList<GpsPunto>> ObtenerPuntosAsync(
        Guid sesionId, DateTime inicio, DateTime? fin, CancellationToken cancellationToken = default);
}
