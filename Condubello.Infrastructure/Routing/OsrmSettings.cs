namespace Condubello.Infrastructure.Routing;

public class OsrmSettings
{
    public const string SectionName = "Osrm";

    /// <summary>URL base del servidor OSRM (osrm-routed).</summary>
    public string BaseUrl { get; set; } = "http://localhost:5001";

    /// <summary>Radio (metros) en que se busca la calle más cercana a cada punto GPS.</summary>
    public int RadioAjusteMetros { get; set; } = 30;
}
