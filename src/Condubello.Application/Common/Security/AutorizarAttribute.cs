namespace Condubello.Application.Common.Security;

/// <summary>
/// Roles que pueden ejecutar el command/query. Sin este atributo, cualquier usuario autenticado puede ejecutarlo.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AutorizarAttribute(params Rol[] roles) : Attribute
{
    public IReadOnlyList<Rol> Roles { get; } = roles;
}
