using Condubello.Api.Common;
using Condubello.Application.Features.Rutas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/rutas")]
[Authorize]
public class RutasController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<RutaDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetRutasQuery(), cancellationToken), "Rutas obtenidas correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<RutaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetRutaByIdQuery(id), cancellationToken), "Ruta obtenida correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<RutaDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateRutaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateRutaCommand(request), cancellationToken),
            "Ruta registrada correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<RutaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateRutaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateRutaCommand(id, request), cancellationToken), "Ruta actualizada correctamente.");

    /// <summary>Reemplaza los puntos de la ruta por el trazado real sobre las calles (requiere OSRM).</summary>
    [HttpPatch("{id:guid}/ajustar-calles")]
    [ProducesResponseType<ApiResponse<RutaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AjustarACalles(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new AjustarRutaACallesCommand(id), cancellationToken), "Ruta ajustada a las calles correctamente.");

    /// <summary>Calcula el trazado por calles entre un punto A y un punto B, sin guardarlo.</summary>
    [HttpPost("calcular-trazado")]
    [ProducesResponseType<ApiResponse<TrazadoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalcularTrazado(CalcularTrazadoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CalcularTrazadoCommand(request), cancellationToken), "Trazado calculado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteRutaCommand(id), cancellationToken), "Ruta eliminada correctamente.");
}
