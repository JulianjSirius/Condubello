using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Condubello.Application.Features.Telemetria;

/// <summary>Imagen SVG del recorrido (rojo, o naranja con alerta) sobre la ruta autorizada (azul).</summary>
public record GetTelemetriaImagenQuery(Guid Id) : IRequest<Result<string>>;

public class GetTelemetriaImagenHandler(
    IRepository<TelemetriaPrueba> repository,
    IRepository<SesionPractica> sesiones,
    IRepository<Ruta> rutas,
    ICurrentUser currentUser) : IRequestHandler<GetTelemetriaImagenQuery, Result<string>>
{
    private const int Ancho = 900, Alto = 560, Margen = 40;

    public async Task<Result<string>> Handle(GetTelemetriaImagenQuery request, CancellationToken cancellationToken)
    {
        var t = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (t is null)
            return Result<string>.NotFound("No se encontró el registro de telemetría.");

        var sesion = await sesiones.GetByIdAsync(t.SesionId, cancellationToken);
        if (currentUser.EsEstudiante && sesion?.EstudianteId != currentUser.Id)
            return Result<string>.Forbidden("No tiene permisos para consultar este registro.");

        var recorrido = GpsReglas.VerticesRuta(t.RutaGps);
        if (recorrido.Count < 2)
            return Result<string>.Invalid("rutaGps", "La telemetría no tiene una ruta GPS para dibujar.");

        var autorizada = new List<(double Lon, double Lat)>();
        if (sesion?.RutaId is { } rutaId)
            autorizada = GpsReglas.VerticesRuta((await rutas.GetByIdAsync(rutaId, cancellationToken))?.GeoJson);

        // Proyección equirectangular ajustada a la latitud media.
        var todos = recorrido.Concat(autorizada).ToList();
        var (minLon, maxLon) = (todos.Min(p => p.Lon), todos.Max(p => p.Lon));
        var (minLat, maxLat) = (todos.Min(p => p.Lat), todos.Max(p => p.Lat));
        var k = Math.Cos((minLat + maxLat) / 2 * Math.PI / 180);
        var w = Math.Max((maxLon - minLon) * k, 1e-6);
        var h = Math.Max(maxLat - minLat, 1e-6);
        var escala = Math.Min((Ancho - 2 * Margen) / w, (Alto - 2 * Margen) / h);
        var offX = (Ancho - w * escala) / 2;
        var offY = (Alto - h * escala) / 2;

        (double X, double Y) Proy((double Lon, double Lat) p) =>
            (offX + (p.Lon - minLon) * k * escala, Alto - (offY + (p.Lat - minLat) * escala));
        static string F(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);
        string Puntos(IEnumerable<(double, double)> ps) => string.Join(' ', ps.Select(Proy).Select(q => $"{F(q.X)},{F(q.Y)}"));

        var colorRecorrido = t.AlertaDesviacion ? "#ef6c00" : "#c62828";
        var (ini, fin) = (Proy(recorrido[0]), Proy(recorrido[^1]));
        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {Ancho} {Alto}\" width=\"{Ancho}\" height=\"{Alto}\" font-family=\"sans-serif\">");
        sb.Append($"<rect width=\"{Ancho}\" height=\"{Alto}\" fill=\"#f4f1ea\"/>");
        if (autorizada.Count >= 2)
            sb.Append($"<polyline points=\"{Puntos(autorizada)}\" fill=\"none\" stroke=\"#1565c0\" stroke-width=\"8\" stroke-opacity=\"0.6\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>");
        sb.Append($"<polyline points=\"{Puntos(recorrido)}\" fill=\"none\" stroke=\"{colorRecorrido}\" stroke-width=\"4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>");
        sb.Append($"<circle cx=\"{F(ini.X)}\" cy=\"{F(ini.Y)}\" r=\"9\" fill=\"#2e7d32\" stroke=\"#fff\" stroke-width=\"2\"/>");
        sb.Append($"<circle cx=\"{F(fin.X)}\" cy=\"{F(fin.Y)}\" r=\"9\" fill=\"#212121\" stroke=\"#fff\" stroke-width=\"2\"/>");
        sb.Append("<g font-size=\"14\" fill=\"#212121\">");
        sb.Append("<rect x=\"12\" y=\"12\" width=\"250\" height=\"" + (autorizada.Count >= 2 ? 78 : 56) + "\" rx=\"6\" fill=\"#fff\" fill-opacity=\"0.9\"/>");
        sb.Append($"<text x=\"24\" y=\"34\"><tspan fill=\"{colorRecorrido}\" font-weight=\"700\">━</tspan> Recorrido real{(t.AlertaDesviacion ? " (desviación)" : "")}</text>");
        sb.Append("<text x=\"24\" y=\"56\"><tspan fill=\"#2e7d32\">●</tspan> Inicio &#160; <tspan fill=\"#212121\">●</tspan> Fin</text>");
        if (autorizada.Count >= 2)
            sb.Append("<text x=\"24\" y=\"78\"><tspan fill=\"#1565c0\" font-weight=\"700\">━</tspan> Ruta autorizada</text>");
        sb.Append("</g></svg>");
        return Result<string>.Success(sb.ToString());
    }
}
