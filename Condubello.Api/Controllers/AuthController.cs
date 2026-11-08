using Condubello.Api.Common;
using Condubello.Application.Features.Auth;
using Condubello.Application.Features.Estudiantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Condubello.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    /// <summary>Inicia sesión con email y contraseña (estudiante, instructor o administrativo).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<LoginResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new LoginCommand(request), cancellationToken), "Inicio de sesión exitoso.");

    /// <summary>Registro público: crea una cuenta de estudiante pendiente de aprobación.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponse<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Registro(CreateEstudianteRequest request, CancellationToken cancellationToken) =>
        HandleResult(await Sender.Send(new RegistrarEstudianteCommand(request), cancellationToken), "Registro recibido. Un administrativo debe aprobarlo antes de que pueda iniciar sesión.");

    /// <summary>Datos del usuario autenticado según el token.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<ApiResponse<UsuarioActualDto>>(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var usuario = new UsuarioActualDto(
            Guid.Parse(User.FindFirst("sub")!.Value),
            User.FindFirst("email")?.Value ?? string.Empty,
            User.FindFirst("name")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty);

        return Ok(ApiResponse<UsuarioActualDto>.Ok(usuario, "Usuario autenticado."));
    }
}
