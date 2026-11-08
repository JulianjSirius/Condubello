using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class Instructor : Persona
{
    public string Cargo { get; set; } = string.Empty;

    public ICollection<SesionPractica> Sesiones { get; set; } = [];
}
