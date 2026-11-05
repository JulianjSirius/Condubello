using Condubello.Domain.Enums;

namespace Condubello.Domain.Common;

/// <summary>Nombres de rol para usar en [Authorize(Roles = ...)].</summary>
public static class Roles
{
    public const string Estudiante = nameof(Rol.Estudiante);
    public const string Instructor = nameof(Rol.Instructor);
    public const string Administrativo = nameof(Rol.Administrativo);
    public const string AdministrativoOInstructor = Administrativo + "," + Instructor;
}
