using System.Linq.Expressions;

namespace Condubello.Application.Features.ValidacionesAntifraude;

public record GetValidacionesAntifraudeQuery : IRequest<Result<IReadOnlyList<ValidacionAntifraudeDto>>>;

public class GetValidacionesAntifraudeHandler(IRepository<ValidacionAntifraude> repository, ICurrentUser currentUser, IMapper mapper)
    : GetAllHandler<GetValidacionesAntifraudeQuery, ValidacionAntifraude, ValidacionAntifraudeDto>(repository, mapper)
{
    protected override Expression<Func<ValidacionAntifraude, bool>>? Filtro()
    {
        if (!currentUser.EsEstudiante)
            return null;

        var yo = currentUser.Id;
        return v => v.Sesion.EstudianteId == yo;
    }
}

public record GetValidacionAntifraudeByIdQuery(Guid Id) : IRequest<Result<ValidacionAntifraudeDto>>, IByIdRequest;

public class GetValidacionAntifraudeByIdHandler(
    IRepository<ValidacionAntifraude> repository,
    IRepository<SesionPractica> sesiones,
    ICurrentUser currentUser,
    IMapper mapper)
    : GetByIdHandler<GetValidacionAntifraudeByIdQuery, ValidacionAntifraude, ValidacionAntifraudeDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró la validación antifraude.";

    protected override async Task<bool> PuedeVerAsync(ValidacionAntifraude entity, CancellationToken cancellationToken)
    {
        if (!currentUser.EsEstudiante)
            return true;

        var yo = currentUser.Id;
        return await sesiones.ExistsAsync(s => s.Id == entity.SesionId && s.EstudianteId == yo, cancellationToken);
    }
}
