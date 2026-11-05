using System.ComponentModel.DataAnnotations;

namespace Condubello.Application.Features.Vehiculos;

public class VehiculoDto
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public EstadoVehiculo EstadoActual { get; set; }
}

public abstract class VehiculoRequestBase
{
    [Required(ErrorMessage = "La placa es obligatoria.")]
    [RegularExpression(@"^\s*[A-Za-z]{3}\d{2}[A-Za-z0-9]\s*$",
        ErrorMessage = "La placa debe tener el formato ABC123 (carro) o ABC12D (moto).")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El modelo no puede superar los 100 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    /// <summary>DISPONIBLE, EN_CLASE o EN_MANTENIMIENTO. Por defecto DISPONIBLE.</summary>
    public EstadoVehiculo? EstadoActual { get; set; }
}

public class CreateVehiculoRequest : VehiculoRequestBase;

public class UpdateVehiculoRequest : VehiculoRequestBase;

public class ActualizarEstadoVehiculoRequest
{
    /// <summary>DISPONIBLE, EN_CLASE o EN_MANTENIMIENTO.</summary>
    [Required(ErrorMessage = "El estado del vehículo es obligatorio.")]
    public EstadoVehiculo? EstadoActual { get; set; }
}
