using System.ComponentModel.DataAnnotations;

namespace Condubello.Application.Features.Personas;

public abstract class PersonaDto
{
    public Guid Id { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public abstract class PersonaRequestBase
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^\s*\d{5,20}\s*$", ErrorMessage = "La cédula debe contener solo números (entre 5 y 20 dígitos).")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;
}

public abstract class CreatePersonaRequest : PersonaRequestBase, IPersonaRequest
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public abstract class UpdatePersonaRequest : PersonaRequestBase, IPersonaRequest
{
    /// <summary>Opcional: si se envía, reemplaza la contraseña actual.</summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    public string? Password { get; set; }
}
