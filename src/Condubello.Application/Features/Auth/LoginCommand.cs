using Condubello.Application.Features.Personas;

namespace Condubello.Application.Features.Auth;

public record LoginCommand(LoginRequest Data) : IRequest<Result<LoginResponseDto>>;

public class LoginHandler(
    IRepository<Estudiante> estudiantes,
    IRepository<Instructor> instructores,
    IRepository<Administrativo> administrativos,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private const string CredencialesInvalidas = "Email o contraseña incorrectos.";

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = PersonaValidator.NormalizarEmail(request.Data.Email);
        var (persona, rol) = await BuscarPersonaAsync(email, cancellationToken);

        if (persona is null || !passwordHasher.Verify(request.Data.Password, persona.PasswordHash))
            return Result<LoginResponseDto>.Unauthorized(CredencialesInvalidas);

        var nombreCompleto = $"{persona.Nombre} {persona.Apellidos}";
        var token = tokenGenerator.Generate(persona.Id, persona.Email, nombreCompleto, rol);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token = token.Token,
            ExpiraEn = token.ExpiraEn,
            UsuarioId = persona.Id,
            NombreCompleto = nombreCompleto,
            Email = persona.Email,
            Rol = rol.ToString()
        });
    }

    private async Task<(Persona? Persona, Rol Rol)> BuscarPersonaAsync(string email, CancellationToken cancellationToken)
    {
        var estudiante = await estudiantes.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        if (estudiante is not null)
            return (estudiante, Rol.Estudiante);

        var instructor = await instructores.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        if (instructor is not null)
            return (instructor, Rol.Instructor);

        var administrativo = await administrativos.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        return (administrativo, Rol.Administrativo);
    }
}
