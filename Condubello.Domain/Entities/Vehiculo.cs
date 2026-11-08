using Condubello.Domain.Common;
using Condubello.Domain.Enums;

namespace Condubello.Domain.Entities;

public class Vehiculo : BaseEntity
{
    public string Placa { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public EstadoVehiculo EstadoActual { get; set; } = EstadoVehiculo.Disponible;
}
