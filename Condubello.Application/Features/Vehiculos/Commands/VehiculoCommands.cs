namespace Condubello.Application.Features.Vehiculos;

internal static class VehiculoReglas
{
    public const string EnClaseAdvertencia =
        "El vehículo está actualmente EN_CLASE; verifique que el cambio no afecte la clase en curso.";

    public static string NormalizarPlaca(string placa) => placa.Trim().ToUpperInvariant();

    public static string Conflicto(string placa) => $"Ya existe un vehículo con la placa {placa}.";
}

[Autorizar(Rol.Administrativo)]
public record CreateVehiculoCommand(CreateVehiculoRequest Data) : IRequest<Result<VehiculoDto>>;

public class CreateVehiculoHandler(
    IRepository<Vehiculo> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateVehiculoCommand, Result<VehiculoDto>>
{
    public async Task<Result<VehiculoDto>> Handle(CreateVehiculoCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var placa = VehiculoReglas.NormalizarPlaca(data.Placa);

        if (await repository.ExistsAsync(v => v.Placa == placa, cancellationToken))
            return Result<VehiculoDto>.Conflict("placa", VehiculoReglas.Conflicto(placa));

        var entity = new Vehiculo
        {
            Placa = placa,
            Modelo = data.Modelo.Trim(),
            EstadoActual = data.EstadoActual ?? EstadoVehiculo.Disponible
        };

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehiculoDto>.Success(mapper.Map<VehiculoDto>(entity));
    }
}

[Autorizar(Rol.Administrativo)]
public record UpdateVehiculoCommand(Guid Id, UpdateVehiculoRequest Data) : IRequest<Result<VehiculoDto>>;

public class UpdateVehiculoHandler(
    IRepository<Vehiculo> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateVehiculoCommand, Result<VehiculoDto>>
{
    public async Task<Result<VehiculoDto>> Handle(UpdateVehiculoCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<VehiculoDto>.NotFound($"No se encontró el vehículo con id {request.Id}.");

        var data = request.Data;
        var placa = VehiculoReglas.NormalizarPlaca(data.Placa);

        if (await repository.ExistsAsync(v => v.Placa == placa && v.Id != entity.Id, cancellationToken))
            return Result<VehiculoDto>.Conflict("placa", VehiculoReglas.Conflicto(placa));

        var warnings = new List<string>();
        if (entity.EstadoActual == EstadoVehiculo.EnClase)
            warnings.Add(VehiculoReglas.EnClaseAdvertencia);

        entity.Placa = placa;
        entity.Modelo = data.Modelo.Trim();
        entity.EstadoActual = data.EstadoActual ?? entity.EstadoActual;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehiculoDto>.Success(mapper.Map<VehiculoDto>(entity), warnings);
    }
}

/// <summary>Actualiza solo el estado del vehículo (permitido también a instructores).</summary>
[Autorizar(Rol.Administrativo, Rol.Instructor)]
public record ActualizarEstadoVehiculoCommand(Guid Id, ActualizarEstadoVehiculoRequest Data) : IRequest<Result<VehiculoDto>>;

public class ActualizarEstadoVehiculoHandler(
    IRepository<Vehiculo> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<ActualizarEstadoVehiculoCommand, Result<VehiculoDto>>
{
    public async Task<Result<VehiculoDto>> Handle(ActualizarEstadoVehiculoCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<VehiculoDto>.NotFound($"No se encontró el vehículo con id {request.Id}.");

        var warnings = new List<string>();
        if (entity.EstadoActual == EstadoVehiculo.EnClase)
            warnings.Add(VehiculoReglas.EnClaseAdvertencia);

        entity.EstadoActual = request.Data.EstadoActual!.Value;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VehiculoDto>.Success(mapper.Map<VehiculoDto>(entity), warnings);
    }
}

[Autorizar(Rol.Administrativo)]
public record DeleteVehiculoCommand(Guid Id) : IRequest<Result<Guid>>, IByIdRequest;

public class DeleteVehiculoHandler(IRepository<Vehiculo> repository, IUnitOfWork unitOfWork)
    : DeleteHandler<DeleteVehiculoCommand, Vehiculo>(repository, unitOfWork)
{
    protected override string NotFoundMessage => "No se encontró el vehículo.";

    protected override Task<Result<Guid>?> ValidateAsync(
        Vehiculo entity, List<string> warnings, CancellationToken cancellationToken)
    {
        if (entity.EstadoActual == EstadoVehiculo.EnClase)
            warnings.Add("Se eliminó un vehículo que estaba EN_CLASE.");

        return Task.FromResult<Result<Guid>?>(null);
    }
}
