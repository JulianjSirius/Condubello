using Condubello.Api.Common;
using Condubello.Application.Features.Auth;
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
