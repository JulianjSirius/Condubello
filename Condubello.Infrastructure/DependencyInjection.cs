using Condubello.Application.Common.Interfaces;
using Condubello.Domain.Interfaces;
using Condubello.Infrastructure.Auth;
using Condubello.Infrastructure.Persistence;
using Condubello.Infrastructure.Geocoding;
using Condubello.Infrastructure.Gps;
using Condubello.Infrastructure.Repositories;
using Condubello.Infrastructure.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Condubello.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        // El GPS es simulado: no hay dispositivos que envíen puntos reales.
        services.AddSingleton<IGpsProvider, SimulatedGpsProvider>();

        services.Configure<MapTilerSettings>(configuration.GetSection(MapTilerSettings.SectionName));
        services.AddHttpClient<IGeocodingProvider, MapTilerGeocodingProvider>(c => c.Timeout = TimeSpan.FromSeconds(15));

        services.Configure<OsrmSettings>(configuration.GetSection(OsrmSettings.SectionName));
        services.AddHttpClient<IRoutingProvider, OsrmRoutingProvider>(c => c.Timeout = TimeSpan.FromSeconds(20));

        return services;
    }
}
