namespace Condubello.Application.Common.Handlers;

public abstract class DeleteHandler<TCommand, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<TCommand, Result<Guid>>
    where TCommand : IRequest<Result<Guid>>, IByIdRequest
    where TEntity : BaseEntity
{
    protected IRepository<TEntity> Repository { get; } = repository;

    protected abstract string NotFoundMessage { get; }

    /// <summary>Reglas antes de eliminar: devuelve un fallo para impedirlo o agrega advertencias.</summary>
    protected virtual Task<Result<Guid>?> ValidateAsync(TEntity entity, List<string> warnings, CancellationToken cancellationToken) =>
        Task.FromResult<Result<Guid>?>(null);

    public async Task<Result<Guid>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await Repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<Guid>.NotFound(NotFoundMessage);

        var warnings = new List<string>();
        var failure = await ValidateAsync(entity, warnings, cancellationToken);
        if (failure is not null)
            return failure;

        Repository.Remove(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entity.Id, warnings);
    }
}
