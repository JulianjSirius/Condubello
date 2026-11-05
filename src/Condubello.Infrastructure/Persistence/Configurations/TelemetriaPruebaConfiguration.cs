using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class TelemetriaPruebaConfiguration : IEntityTypeConfiguration<TelemetriaPrueba>
{
    public void Configure(EntityTypeBuilder<TelemetriaPrueba> builder)
    {
        builder.ToTable("telemetria_pruebas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ResumenTelemetria).HasColumnType("jsonb");
        builder.Property(t => t.RutaGps).HasColumnType("jsonb");

        builder.HasOne(t => t.Sesion)
            .WithMany(s => s.Telemetrias)
            .HasForeignKey(t => t.SesionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
