using System.Text;
using Condubello.Application.Common.Interfaces;
using Condubello.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Condubello.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtSettings> options) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public TokenResult Generate(Guid usuarioId, string email, string nombreCompleto, Rol rol)
    {
        var expiraEn = DateTime.UtcNow.AddMinutes(_settings.ExpiraMinutos);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiraEn,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuarioId.ToString(),
                [JwtRegisteredClaimNames.Email] = email,
                [JwtRegisteredClaimNames.Name] = nombreCompleto,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                ["role"] = rol.ToString()
            }
        };

        return new TokenResult(new JsonWebTokenHandler().CreateToken(descriptor), expiraEn);
    }
}
