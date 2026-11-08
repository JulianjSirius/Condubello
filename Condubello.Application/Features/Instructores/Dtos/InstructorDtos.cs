using System.ComponentModel.DataAnnotations;
using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Instructores;

public class InstructorDto : PersonaDto
{
    public string Cargo { get; set; } = string.Empty;
}

public class CreateInstructorRequest : CreatePersonaRequest
{
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El cargo no puede superar los 100 caracteres.")]
    public string Cargo { get; set; } = string.Empty;
}

public class UpdateInstructorRequest : UpdatePersonaRequest
{
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El cargo no puede superar los 100 caracteres.")]
    public string Cargo { get; set; } = string.Empty;
}
