namespace Condubello.Infrastructure.Geocoding;

public class MapTilerSettings
{
    public const string SectionName = "MapTiler";

    /// <summary>Clave de https://cloud.maptiler.com. Sin ella la geocodificación queda desactivada.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
