namespace Condubello.Application.Common.Interfaces;

public record TokenResult(string Token, DateTime ExpiraEn);

public interface IJwtTokenGenerator
{
    TokenResult Generate(Guid usuarioId, string email, string nombreCompleto, Rol rol);
}
