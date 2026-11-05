using Condubello.Api.Common;
using Condubello.Application.Features.SesionesPracticas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/sesiones-practicas")]
[Authorize]
public class SesionesPracticasController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<SesionPracticaDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetSesionesPracticasQuery(), cancellationToken), "Sesiones prácticas obtenidas correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<SesionPracticaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetSesionPracticaByIdQuery(id), cancellationToken), "Sesión práctica obtenida correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<SesionPracticaDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateSesionPracticaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateSesionPracticaCommand(request), cancellationToken),
            "Sesión práctica registrada correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<SesionPracticaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateSesionPracticaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateSesionPracticaCommand(id, request), cancellationToken), "Sesión práctica actualizada correctamente.");

    /// <summary>Actualiza fechas, estado y calificación del instructor. El instructor solo puede actualizar sus sesiones.</summary>
    [HttpPatch("{id:guid}/seguimiento")]
    [ProducesResponseType<ApiResponse<SesionPracticaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarSeguimiento(Guid id, ActualizarSeguimientoSesionRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new ActualizarSeguimientoSesionCommand(id, request), cancellationToken), "Seguimiento de la sesión actualizado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteSesionPracticaCommand(id), cancellationToken), "Sesión práctica eliminada correctamente.");
}
