using Condubello.Api.Common;
using Condubello.Application.Features.Vehiculos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/vehiculos")]
[Authorize]
public class VehiculosController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<VehiculoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetVehiculosQuery(), cancellationToken), "Vehículos obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetVehiculoByIdQuery(id), cancellationToken), "Vehículo obtenido correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<VehiculoDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateVehiculoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateVehiculoCommand(request), cancellationToken),
            "Vehículo registrado correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateVehiculoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateVehiculoCommand(id, request), cancellationToken), "Vehículo actualizado correctamente.");

    /// <summary>Actualiza solo el estado del vehículo (administrativo o instructor).</summary>
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType<ApiResponse<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarEstado(Guid id, ActualizarEstadoVehiculoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new ActualizarEstadoVehiculoCommand(id, request), cancellationToken), "Estado del vehículo actualizado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteVehiculoCommand(id), cancellationToken), "Vehículo eliminado correctamente.");
}
