namespace Condubello.Application.Common.Handlers;

public abstract class GetByIdHandler<TQuery, TEntity, TDto>(IRepository<TEntity> repository, IMapper mapper)
    : IRequestHandler<TQuery, Result<TDto>>
    where TQuery : IRequest<Result<TDto>>, IByIdRequest
    where TEntity : BaseEntity
{
    protected abstract string NotFoundMessage { get; }

    /// <summary>Indica si el usuario actual puede ver el registro encontrado.</summary>
    protected virtual Task<bool> PuedeVerAsync(TEntity entity, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public async Task<Result<TDto>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<TDto>.NotFound(NotFoundMessage);

        if (!await PuedeVerAsync(entity, cancellationToken))
            return Result<TDto>.Forbidden("No tiene permisos para consultar este registro.");

        return Result<TDto>.Success(mapper.Map<TDto>(entity));
    }
}
