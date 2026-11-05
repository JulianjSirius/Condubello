using Condubello.Api.Common;
using Condubello.Application.Features.Administrativos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/administrativos")]
[Authorize]
public class AdministrativosController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<AdministrativoDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetAdministrativosQuery(), cancellationToken), "Administrativos obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<AdministrativoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetAdministrativoByIdQuery(id), cancellationToken), "Administrativo obtenido correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<AdministrativoDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateAdministrativoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateAdministrativoCommand(request), cancellationToken),
            "Administrativo registrado correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<AdministrativoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateAdministrativoRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateAdministrativoCommand(id, request), cancellationToken), "Administrativo actualizado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteAdministrativoCommand(id), cancellationToken), "Administrativo eliminado correctamente.");
}
