namespace Condubello.Application.Features.Geocodificacion;

public record DireccionDto(string Texto, double Latitud, double Longitud);

[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record GeocodificarInversaQuery(double Latitud, double Longitud) : IRequest<Result<DireccionDto>>;

public class GeocodificarInversaHandler(IGeocodingProvider geocoding) : IRequestHandler<GeocodificarInversaQuery, Result<DireccionDto>>
{
    public async Task<Result<DireccionDto>> Handle(GeocodificarInversaQuery request, CancellationToken cancellationToken)
    {
        if (request.Latitud is < -90 or > 90 || request.Longitud is < -180 or > 180)
            return Result<DireccionDto>.Invalid("coordenadas", "La latitud debe estar entre -90 y 90 y la longitud entre -180 y 180.");
        if (!geocoding.Configurado)
            return Result<DireccionDto>.Invalid("geocodificacion", "La geocodificación no está configurada (falta MapTiler:ApiKey).");

        var d = await geocoding.InversaAsync(request.Latitud, request.Longitud, cancellationToken);
        return d is null
            ? Result<DireccionDto>.NotFound("No se encontró una dirección para esa coordenada.")
            : Result<DireccionDto>.Success(new DireccionDto(d.Texto, d.Latitud, d.Longitud));
    }
}

[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record BuscarDireccionQuery(string Texto) : IRequest<Result<DireccionDto>>;

public class BuscarDireccionHandler(IGeocodingProvider geocoding) : IRequestHandler<BuscarDireccionQuery, Result<DireccionDto>>
{
    public async Task<Result<DireccionDto>> Handle(BuscarDireccionQuery request, CancellationToken cancellationToken)
    {
        var texto = request.Texto?.Trim() ?? string.Empty;
        if (texto.Length is < 3 or > 200)
            return Result<DireccionDto>.Invalid("texto", "La dirección debe tener entre 3 y 200 caracteres.");
        if (!geocoding.Configurado)
            return Result<DireccionDto>.Invalid("geocodificacion", "La geocodificación no está configurada (falta MapTiler:ApiKey).");

        var d = await geocoding.BuscarAsync(texto, cancellationToken);
        return d is null
            ? Result<DireccionDto>.NotFound("No se encontró esa dirección en Colombia.")
            : Result<DireccionDto>.Success(new DireccionDto(d.Texto, d.Latitud, d.Longitud));
    }
}
