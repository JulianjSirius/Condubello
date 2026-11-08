namespace Condubello.Application.Features.Personas;

public interface IPersonaRequest
{
    string Cedula { get; }
    string Email { get; }
    string? Password { get; }
}

public interface IPersonaCommand
{
    IPersonaRequest Data { get; }
}
