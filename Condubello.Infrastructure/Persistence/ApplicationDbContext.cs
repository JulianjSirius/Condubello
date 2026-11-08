using Condubello.Domain.Entities;
using Condubello.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Condubello.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
    public DbSet<Instructor> Instructores => Set<Instructor>();
    public DbSet<Administrativo> Administrativos => Set<Administrativo>();
    public DbSet<ControlHorasEstudiante> ControlHorasEstudiantes => Set<ControlHorasEstudiante>();
    public DbSet<SesionPractica> SesionesPracticas => Set<SesionPractica>();
    public DbSet<ValidacionAntifraude> ValidacionesAntifraude => Set<ValidacionAntifraude>();
    public DbSet<TelemetriaPrueba> TelemetriaPruebas => Set<TelemetriaPrueba>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Ruta> Rutas => Set<Ruta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
