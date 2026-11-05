using Condubello.Api.Common;
using Condubello.Application.Features.Instructores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/instructores")]
[Authorize]
public class InstructoresController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<InstructorDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetInstructoresQuery(), cancellationToken), "Instructores obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<InstructorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetInstructorByIdQuery(id), cancellationToken), "Instructor obtenido correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<InstructorDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateInstructorRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateInstructorCommand(request), cancellationToken),
            "Instructor registrado correctamente.", StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<InstructorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, UpdateInstructorRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new UpdateInstructorCommand(id, request), cancellationToken), "Instructor actualizado correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteInstructorCommand(id), cancellationToken), "Instructor eliminado correctamente.");
}
