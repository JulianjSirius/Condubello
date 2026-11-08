namespace Condubello.Application.Features.Administrativos;

[Autorizar(Rol.Administrativo)]
public record GetAdministrativosQuery : IRequest<Result<IReadOnlyList<AdministrativoDto>>>;

public class GetAdministrativosHandler(IRepository<Administrativo> repository, IMapper mapper)
    : GetAllHandler<GetAdministrativosQuery, Administrativo, AdministrativoDto>(repository, mapper);

[Autorizar(Rol.Administrativo)]
public record GetAdministrativoByIdQuery(Guid Id) : IRequest<Result<AdministrativoDto>>, IByIdRequest;

public class GetAdministrativoByIdHandler(IRepository<Administrativo> repository, IMapper mapper)
    : GetByIdHandler<GetAdministrativoByIdQuery, Administrativo, AdministrativoDto>(repository, mapper)
{
    protected override string NotFoundMessage => "No se encontró el administrativo.";
}
