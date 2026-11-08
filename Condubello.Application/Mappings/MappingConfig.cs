using Condubello.Application.Features.ControlHoras;
using Condubello.Application.Features.Rutas;
using Condubello.Application.Features.Telemetria;
using Mapster;

namespace Condubello.Application.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ControlHorasEstudiante, ControlHorasDto>()
            .Map(d => d.HorasPendientes, s => s.HorasRealizadas >= s.HorasRequeridas ? 0 : s.HorasRequeridas - s.HorasRealizadas)
            .Map(d => d.Completado, s => s.HorasRealizadas >= s.HorasRequeridas);

        config.NewConfig<TelemetriaPrueba, TelemetriaPruebaDto>()
            .Map(d => d.ResumenTelemetria, s => JsonHelper.Parse(s.ResumenTelemetria))
            .Map(d => d.RutaGps, s => JsonHelper.Parse(s.RutaGps));

        config.NewConfig<Ruta, RutaDto>()
            .Map(d => d.GeoJson, s => JsonHelper.Parse(s.GeoJson));
    }
}
