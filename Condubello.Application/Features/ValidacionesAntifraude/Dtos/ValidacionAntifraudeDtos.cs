using System.ComponentModel.DataAnnotations;

namespace Condubello.Application.Features.ValidacionesAntifraude;

public class ValidacionAntifraudeDto
{
    public Guid Id { get; set; }
    public Guid SesionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string UrlFotoCapturada { get; set; } = string.Empty;
    public bool EstadoValidacion { get; set; }
}

public abstract class ValidacionAntifraudeRequestBase
{
    /// <summary>Opcional: si no se envía se usa la fecha/hora actual.</summary>
    public DateTime? Timestamp { get; set; }

    [Required(ErrorMessage = "La URL de la foto capturada es obligatoria.")]
    [Url(ErrorMessage = "La URL de la foto capturada no es válida.")]
    [StringLength(500, ErrorMessage = "La URL de la foto no puede superar los 500 caracteres.")]
    public string UrlFotoCapturada { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado de la validación es obligatorio.")]
    public bool? EstadoValidacion { get; set; }
}

public class CreateValidacionAntifraudeRequest : ValidacionAntifraudeRequestBase
{
    [Required(ErrorMessage = "La sesión es obligatoria.")]
    public Guid? SesionId { get; set; }
}

public class UpdateValidacionAntifraudeRequest : ValidacionAntifraudeRequestBase;
