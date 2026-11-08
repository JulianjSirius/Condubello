using System.Reflection;
using Condubello.Application.Common.Behaviors;
using Condubello.Application.Features.Personas;
using Condubello.Application.Features.SesionesPracticas;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Condubello.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = configuration["MediatR:LicenseKey"];
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
        });

        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddScoped<PersonaValidator>();
        services.AddScoped<SesionPracticaValidator>();

        return services;
    }
}
