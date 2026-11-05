namespace Condubello.Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Clave HMAC de al menos 32 caracteres.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiraMinutos { get; set; } = 480;
}
