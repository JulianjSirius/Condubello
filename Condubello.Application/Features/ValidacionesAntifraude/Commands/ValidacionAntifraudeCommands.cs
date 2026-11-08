namespace Condubello.Application.Features.ValidacionesAntifraude;

internal static class ValidacionAntifraudeReglas
{
    public static List<string> Advertencias(ValidacionAntifraude validacion) =>
        validacion.EstadoValidacion
            ? []
            : ["Validación fallida: la foto capturada no coincide con el estudiante (posible suplantación). Considere marcar la sesión como FRAUDE_DETECTADO."];

    public static void Aplicar(ValidacionAntifraude entity, ValidacionAntifraudeRequestBase data)
    {
        entity.Timestamp = data.Timestamp?.ToUtc() ?? DateTime.UtcNow;
        entity.UrlFotoCapturada = data.UrlFotoCapturada.Trim();
        entity.EstadoValidacion = data.EstadoValidacion!.Value;
    }
}

[Autorizar(Rol.Administrativo)]
public record CreateValidacionAntifraudeCommand(CreateValidacionAntifraudeRequest Data)
    : IRequest<Result<ValidacionAntifraudeDto>>;

public class CreateValidacionAntifraudeHandler(
    IRepository<ValidacionAntifraude> repository,
    IRepository<SesionPractica> sesiones,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateValidacionAntifraudeCommand, Result<ValidacionAntifraudeDto>>
{
    public async Task<Result<ValidacionAntifraudeDto>> Handle(
        CreateValidacionAntifraudeCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var sesionId = data.SesionId!.Value;

        var sesion = await sesiones.GetByIdAsync(sesionId, cancellationToken);
        if (sesion is null)
            return Result<ValidacionAntifraudeDto>.NotFound($"No se encontró la sesión práctica con id {sesionId}.");

        if (sesion.EstadoPrueba == EstadoPrueba.Cancelada)
            return Result<ValidacionAntifraudeDto>.Invalid("sesionId", "No se pueden registrar validaciones en una sesión CANCELADA.");

        var entity = new ValidacionAntifraude { SesionId = sesionId };
        ValidacionAntifraudeReglas.Aplicar(entity, data);

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ValidacionAntifraudeDto>.Success(
            mapper.Map<ValidacionAntifraudeDto>(entity), ValidacionAntifraudeReglas.Advertencias(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record UpdateValidacionAntifraudeCommand(Guid Id, UpdateValidacionAntifraudeRequest Data)
    : IRequest<Result<ValidacionAntifraudeDto>>;

public class UpdateValidacionAntifraudeHandler(
    IRepository<ValidacionAntifraude> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateValidacionAntifraudeCommand, Result<ValidacionAntifraudeDto>>
{
    public async Task<Result<ValidacionAntifraudeDto>> Handle(
        UpdateValidacionAntifraudeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<ValidacionAntifraudeDto>.NotFound($"No se encontró la validación antifraude con id {request.Id}.");

        ValidacionAntifraudeReglas.Aplicar(entity, request.Data);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ValidacionAntifraudeDto>.Success(
            mapper.Map<ValidacionAntifraudeDto>(entity), ValidacionAntifraudeReglas.Advertencias(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteValidacionAntifraudeCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteValidacionAntifraudeHandler(IRepository<ValidacionAntifraude> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteValidacionAntifraudeCommand, ValidacionAntifraude>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró la validación antifraude.";
}
