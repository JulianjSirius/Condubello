using System.Linq.Expressions;

namespace Condubello.Application.Common.Handlers;

public abstract class GetAllHandler<TQuery, TEntity, TDto>(IRepository<TEntity> repository, IMapper mapper)
    : IRequestHandler<TQuery, Result<IReadOnlyList<TDto>>>
    where TQuery : IRequest<Result<IReadOnlyList<TDto>>>
    where TEntity : BaseEntity
{
    /// <summary>Restringe los registros que puede ver el usuario actual; null devuelve todos.</summary>
    protected virtual Expression<Func<TEntity, bool>>? Filtro() => null;

    public async Task<Result<IReadOnlyList<TDto>>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var filtro = Filtro();
        var entities = filtro is null
            ? await repository.GetAllAsync(cancellationToken)
            : await repository.ListAsync(filtro, cancellationToken);
        return Result<IReadOnlyList<TDto>>.Success(mapper.Map<List<TDto>>(entities));
    }
}
