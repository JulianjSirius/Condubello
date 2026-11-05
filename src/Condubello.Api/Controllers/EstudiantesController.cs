using Condubello.Api.Common;
using Condubello.Application.Features.Estudiantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/estudiantes")]
[Authorize]
public class EstudiantesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<EstudianteDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetEstudiantesQuery(), cancellationToken), "Estudiantes obtenidos correctamente.");

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetEstudianteByIdQuery(id), cancellationToken), "Estudiante obtenido correctamente.");

    /// <summary>Perfil del estudiante autenticado.</summary>
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMiPerfil(CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new GetMiPerfilEstudianteQuery(), cancellationToken), "Perfil obtenido correctamente.");

    /// <summary>El estudiante autenticado cambia su email y/o contraseña (requiere la contraseña actual).</summary>
    [HttpPut("me/cuenta")]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarMiCuenta(ActualizarMiCuentaRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new ActualizarMiCuentaCommand(request), cancellationToken), "Cuenta actualizada correctamente.");

    [HttpPost]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateEstudianteRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new CreateEstudianteCommand(request), cancellationToken),
            "Estudiante registrado correctamente.", StatusCodes.Status201Created);

    /// <summary>Restablece la contraseña de un estudiante (administrativo).</summary>
    [HttpPatch("{id:guid}/password")]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RestablecerPassword(Guid id, RestablecerPasswordRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new RestablecerPasswordEstudianteCommand(id, request), cancellationToken),
            "Contraseña restablecida correctamente.");

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<Guid>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new DeleteEstudianteCommand(id), cancellationToken), "Estudiante eliminado correctamente.");
}
