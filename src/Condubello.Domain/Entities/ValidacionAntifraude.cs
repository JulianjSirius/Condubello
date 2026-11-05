using Condubello.Domain.Common;

namespace Condubello.Domain.Entities;

public class ValidacionAntifraude : BaseEntity
{
    public Guid SesionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UrlFotoCapturada { get; set; } = string.Empty;
    public bool EstadoValidacion { get; set; }

    public SesionPractica Sesion { get; set; } = null!;
}
