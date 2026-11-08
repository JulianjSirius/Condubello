namespace Condubello.Application.Features.Personas;

public abstract class CreatePersonaHandler<TCommand, TEntity, TDto>(
    IRepository<TEntity> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<TCommand, Result<TDto>>
    where TCommand : IRequest<Result<TDto>>, IPersonaCommand
    where TEntity : Persona
{
    protected abstract Rol Rol { get; }

    public async Task<Result<TDto>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;

        var errores = await validator.ValidarUnicidadAsync(Rol, data.Cedula, data.Email, null, cancellationToken);
        if (errores.Count > 0)
            return Result<TDto>.Conflict(errores);

        var entity = mapper.Map<TEntity>(data);
        PersonaValidator.Normalizar(entity);
        entity.PasswordHash = passwordHasher.Hash(data.Password!);

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TDto>.Success(mapper.Map<TDto>(entity));
    }
}

public abstract class UpdatePersonaHandler<TCommand, TEntity, TDto>(
    IRepository<TEntity> repository,
    PersonaValidator validator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<TCommand, Result<TDto>>
    where TCommand : IRequest<Result<TDto>>, IPersonaCommand, IByIdRequest
    where TEntity : Persona
{
    protected abstract Rol Rol { get; }

    public async Task<Result<TDto>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<TDto>.NotFound($"No se encontró el {PersonaValidator.NombreRol(Rol)} con id {request.Id}.");

        var data = request.Data;

        var errores = await validator.ValidarUnicidadAsync(Rol, data.Cedula, data.Email, entity.Id, cancellationToken);
        if (errores.Count > 0)
            return Result<TDto>.Conflict(errores);

        mapper.Map(data, entity, data.GetType(), typeof(TEntity));
        PersonaValidator.Normalizar(entity);
        if (!string.IsNullOrWhiteSpace(data.Password))
            entity.PasswordHash = passwordHasher.Hash(data.Password);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TDto>.Success(mapper.Map<TDto>(entity));
    }
}
