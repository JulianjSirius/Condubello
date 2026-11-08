namespace Condubello.Application.Common.Interfaces;

/// <summary>Usuario autenticado que realiza la petición (según el token).</summary>
public interface ICurrentUser
{
    Guid Id { get; }
    Rol Rol { get; }

    bool EsAdministrativo => Rol == Rol.Administrativo;
    bool EsInstructor => Rol == Rol.Instructor;
    bool EsEstudiante => Rol == Rol.Estudiante;
}
