using System.ComponentModel.DataAnnotations;

namespace Condubello.Application.Features.ControlHoras;

public class ControlHorasDto
{
    public Guid Id { get; set; }
    public Guid EstudianteId { get; set; }
    public string TipoLicencia { get; set; } = string.Empty;
    public int HorasRequeridas { get; set; }
    public decimal HorasRealizadas { get; set; }
    public decimal HorasPendientes { get; set; }
    public bool Completado { get; set; }
}

public abstract class ControlHorasRequestBase
{
    [Required(ErrorMessage = "El tipo de licencia es obligatorio.")]
    [RegularExpression("(?i)^(A1|A2|B1|B2|B3|C1|C2|C3)$",
        ErrorMessage = "El tipo de licencia debe ser A1, A2, B1, B2, B3, C1, C2 o C3.")]
    public string TipoLicencia { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Las horas requeridas deben estar entre 1 y 500.")]
    public int HorasRequeridas { get; set; }

    [Range(typeof(decimal), "0", "1000", ErrorMessage = "Las horas realizadas deben estar entre 0 y 1000.")]
    public decimal HorasRealizadas { get; set; }
}

public class CreateControlHorasRequest : ControlHorasRequestBase
{
    [Required(ErrorMessage = "El estudiante es obligatorio.")]
    public Guid? EstudianteId { get; set; }
}

public class UpdateControlHorasRequest : ControlHorasRequestBase;
