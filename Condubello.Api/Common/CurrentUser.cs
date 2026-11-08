using Condubello.Application.Common.Interfaces;
using Condubello.Domain.Enums;

namespace Condubello.Api.Common;

/// <summary>Lee el usuario autenticado de los claims del token (sub y role).</summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid Id => Guid.Parse(Claim("sub"));

    public Rol Rol => Enum.Parse<Rol>(Claim("role"));

    private string Claim(string tipo) =>
        httpContextAccessor.HttpContext?.User.FindFirst(tipo)?.Value
        ?? throw new InvalidOperationException($"El usuario actual no tiene el claim '{tipo}'.");
}
