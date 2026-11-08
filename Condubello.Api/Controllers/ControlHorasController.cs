using Condubello.Api.Common;
using Condubello.Application.Features.ControlHoras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/control-horas")]
[Authorize]
public class ControlHorasController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<ControlHorasDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetControlHorasQuery(), cancellationToken), "Controles de horas obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ControlHorasDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetControlHorasByIdQuery(id), cancellationToken), "Control de horas obtenido correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<ControlHorasDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateControlHorasRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateControlHorasCommand(request), cancellationToken),
            "Control de horas registrado correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ControlHorasDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateControlHorasRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateControlHorasCommand(id, request), cancellationToken), "Control de horas actualizado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteControlHorasCommand(id), cancellationToken), "Control de horas eliminado correctamente.");
}
