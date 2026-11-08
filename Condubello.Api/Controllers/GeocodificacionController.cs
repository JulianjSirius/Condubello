using Condubello.Api.Common;
using Condubello.Application.Features.Geocodificacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/geocodificacion")]
[Authorize]
public class GeocodificacionController : ApiControllerBase
{
    /// <summary>De coordenada a dirección (con caché en base de datos).</summary>
    [HttpGet("inversa")]
    [ProducesResponseType<ApiResponse<DireccionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Inversa([FromQuery] double lat, [FromQuery] double lon, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GeocodificarInversaQuery(lat, lon), cancellationToken), "Dirección obtenida correctamente.");

    /// <summary>De dirección a coordenada, limitado a Colombia (con caché en base de datos).</summary>
    [HttpGet("buscar")]
    [ProducesResponseType<ApiResponse<DireccionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string texto, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new BuscarDireccionQuery(texto), cancellationToken), "Dirección encontrada correctamente.");
}
