using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class Administrativo : Persona
{
    public string Cargo { get; set; } = string.Empty;
}
