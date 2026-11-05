using Condubello.Api.Common;
using Condubello.Application.Features.ValidacionesAntifraude;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/validaciones-antifraude")]
[Authorize]
public class ValidacionesAntifraudeController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<ValidacionAntifraudeDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetValidacionesAntifraudeQuery(), cancellationToken), "Validaciones antifraude obtenidas correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ValidacionAntifraudeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetValidacionAntifraudeByIdQuery(id), cancellationToken), "Validación antifraude obtenida correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<ValidacionAntifraudeDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateValidacionAntifraudeRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateValidacionAntifraudeCommand(request), cancellationToken),
            "Validación antifraude registrada correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ValidacionAntifraudeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateValidacionAntifraudeRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateValidacionAntifraudeCommand(id, request), cancellationToken), "Validación antifraude actualizada correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteValidacionAntifraudeCommand(id), cancellationToken), "Validación antifraude eliminada correctamente.");
}
