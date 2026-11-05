using Condubello.Api.Common;
using Condubello.Application.Features.Telemetria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/telemetria")]
[Authorize]
public class TelemetriaController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<TelemetriaPruebaDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetTelemetriaPruebasQuery(), cancellationToken), "Registros de telemetría obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<TelemetriaPruebaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetTelemetriaPruebaByIdQuery(id), cancellationToken), "Registro de telemetría obtenido correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<TelemetriaPruebaDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateTelemetriaPruebaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateTelemetriaPruebaCommand(request), cancellationToken),
            "Registro de telemetría guardado correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<TelemetriaPruebaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateTelemetriaPruebaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateTelemetriaPruebaCommand(id, request), cancellationToken), "Registro de telemetría actualizado correctamente.");

    /// <summary>Actualiza solo la ruta GPS. El instructor solo puede actualizar la telemetría de sus sesiones.</summary>
    [HttpPatch("{id:guid}/ruta-gps")]
    [ProducesResponseType<ApiResponse<TelemetriaPruebaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarRutaGps(Guid id, ActualizarRutaGpsRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new ActualizarRutaGpsCommand(id, request), cancellationToken), "Ruta GPS actualizada correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteTelemetriaPruebaCommand(id), cancellationToken), "Registro de telemetría eliminado correctamente.");
}
