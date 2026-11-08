namespace Condubello.Application.Common.Interfaces;

/// <summary>Dirección legible con su coordenada.</summary>
public record DireccionGeocodificada(string Texto, double Latitud, double Longitud);

/// <summary>Convierte coordenadas en direcciones y viceversa. Devuelve null si no hay resultado o el servicio falla.</summary>
public interface IGeocodingProvider
{
    /// <summary>False si falta la clave del proveedor.</summary>
    bool Configurado { get; }

    Task<DireccionGeocodificada?> InversaAsync(double latitud, double longitud, CancellationToken cancellationToken = default);
    Task<DireccionGeocodificada?> BuscarAsync(string texto, CancellationToken cancellationToken = default);
}
