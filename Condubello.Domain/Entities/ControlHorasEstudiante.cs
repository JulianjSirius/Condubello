using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class ControlHorasEstudiante : BaseEntity
{
    public Guid EstudianteId { get; set; }
    public string TipoLicencia { get; set; } = string.Empty;
    public int HorasRequeridas { get; set; }
    public decimal HorasRealizadas { get; set; }

    public Estudiante Estudiante { get; set; } = null!;
}
