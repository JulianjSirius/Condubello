using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

[Autorizar(Rol.Administrativo)]
public record SincronizarGpsCommand(SincronizarGpsRequest Data) : IRequest<Result<SincronizacionGpsDto>>;

public class SincronizarGpsHandler(
    IRepository<SesionPractica> sesiones,
    IRepository<TelemetriaPrueba> telemetrias,
    IRepository<Ruta> rutas,
    IGpsProvider gps,
    IRoutingProvider routing,
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

        int sincronizadas = 0, conAlerta = 0, sinDatos = 0, sinAjustar = 0;
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

            // Se ajusta cada punto a la calle más cercana para que el ruido del GPS no genere falsas alertas.
            var ajustados = await routing.AjustarPuntosAsync(puntos, cancellationToken);
            if (ajustados is null)
                sinAjustar++;
            var puntosEvaluados = ajustados ?? puntos;
            var desviacion = GpsReglas.HayDesviacion(puntosEvaluados, ruta);

            var telemetria = await telemetrias.FirstOrDefaultAsync(t => t.SesionId == sesion.Id, cancellationToken);
            var esNueva = telemetria is null;
            telemetria ??= new TelemetriaPrueba { SesionId = sesion.Id };

            telemetria.RutaGps = GpsReglas.ConstruirLineString(puntosEvaluados);
            telemetria.AlertaDesviacion = desviacion;
            telemetria.ResumenTelemetria ??= JsonSerializer.Serialize(new
            {
                fuente = "gps-sincronizado",
                puntos = puntos.Count,
                ajustadoACalles = ajustados is not null,
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

        var warnings = new List<string>();
        if (conAlerta > 0)
            warnings.Add($"{conAlerta} sesión(es) con desviación de la ruta autorizada.");
        if (sinAjustar > 0)
            warnings.Add($"{sinAjustar} sesión(es) se guardaron con los puntos GPS originales: OSRM no respondió o no pudo ajustarlas a las calles.");
        return Result<SincronizacionGpsDto>.Success(new SincronizacionGpsDto(sincronizadas, conAlerta, sinDatos), warnings.Count > 0 ? warnings : null);
    }
}
