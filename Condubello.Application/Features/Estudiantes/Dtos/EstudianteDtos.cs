using System.ComponentModel.DataAnnotations;
using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Estudiantes;

public class EstudianteDto : PersonaDto
{
    public DateTime FechaRegistro { get; set; }
    public bool Aprobado { get; set; }
}

public class CreateEstudianteRequest : CreatePersonaRequest;

public class RestablecerPasswordRequest
{
    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string PasswordNueva { get; set; } = string.Empty;
}

public class ActualizarMiCuentaRequest
{
    /// <summary>Opcional: nuevo email de acceso.</summary>
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string PasswordActual { get; set; } = string.Empty;

    /// <summary>Opcional: si se envía, reemplaza la contraseña actual.</summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string? PasswordNueva { get; set; }
}
