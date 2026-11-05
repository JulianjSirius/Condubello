namespace Condubello.Application.Features.Personas;

/// <summary>
/// Reglas comunes de estudiantes, instructores y administrativos.
/// La cédula es única por tabla; el email es único en las tres porque el login es común.
/// </summary>
public class PersonaValidator(
    IRepository<Estudiante> estudiantes,
    IRepository<Instructor> instructores,
    IRepository<Administrativo> administrativos)
{
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    public static string NombreRol(Rol rol) => rol switch
    {
        Rol.Estudiante => "estudiante",
        Rol.Instructor => "instructor",
        _ => "administrativo"
    };

    public static void Normalizar(Persona persona)
    {
        persona.Cedula = persona.Cedula.Trim();
        persona.Nombre = persona.Nombre.Trim();
        persona.Apellidos = persona.Apellidos.Trim();
        persona.Email = NormalizarEmail(persona.Email);
    }

    public async Task<List<ApiError>> ValidarUnicidadAsync(
        Rol rol, string cedula, string email, Guid? excluirId, CancellationToken cancellationToken)
    {
        var errores = new List<ApiError>();
        cedula = cedula.Trim();

        var cedulaExiste = rol switch
        {
            Rol.Estudiante => await estudiantes.ExistsAsync(p => p.Cedula == cedula && p.Id != excluirId, cancellationToken),
            Rol.Instructor => await instructores.ExistsAsync(p => p.Cedula == cedula && p.Id != excluirId, cancellationToken),
            _ => await administrativos.ExistsAsync(p => p.Cedula == cedula && p.Id != excluirId, cancellationToken)
        };
        if (cedulaExiste)
            errores.Add(new ApiError("cedula", $"Ya existe un {NombreRol(rol)} registrado con la cédula {cedula}."));

        var emailNormalizado = NormalizarEmail(email);
        var emailExiste =
            await estudiantes.ExistsAsync(p => p.Email == emailNormalizado && p.Id != excluirId, cancellationToken) ||
            await instructores.ExistsAsync(p => p.Email == emailNormalizado && p.Id != excluirId, cancellationToken) ||
            await administrativos.ExistsAsync(p => p.Email == emailNormalizado && p.Id != excluirId, cancellationToken);
        if (emailExiste)
            errores.Add(new ApiError("email", $"El email {emailNormalizado} ya está registrado en el sistema."));

        return errores;
    }
}
