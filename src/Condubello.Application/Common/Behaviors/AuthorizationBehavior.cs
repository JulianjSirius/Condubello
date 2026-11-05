using System.Reflection;
using Condubello.Application.Common.Security;

namespace Condubello.Application.Common.Behaviors;

/// <summary>Rechaza el command/query si el rol del usuario no está en su <see cref="AutorizarAttribute"/>.</summary>
public class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IFailureResult<TResponse>
{
    private static readonly AutorizarAttribute? Autorizar = typeof(TRequest).GetCustomAttribute<AutorizarAttribute>();

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (Autorizar is not null && !Autorizar.Roles.Contains(currentUser.Rol))
            return Task.FromResult(TResponse.Forbidden("No tiene permisos para realizar esta acción."));

        return next(cancellationToken);
    }
}
