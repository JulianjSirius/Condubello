using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class ControlHorasEstudianteConfiguration : IEntityTypeConfiguration<ControlHorasEstudiante>
{
    public void Configure(EntityTypeBuilder<ControlHorasEstudiante> builder)
    {
        builder.ToTable("control_horas_estudiante");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.TipoLicencia).HasMaxLength(10).IsRequired();
        builder.Property(c => c.HorasRealizadas).HasPrecision(6, 2);

        builder.HasIndex(c => new { c.EstudianteId, c.TipoLicencia }).IsUnique();

        builder.HasOne(c => c.Estudiante)
            .WithMany(e => e.ControlHoras)
            .HasForeignKey(c => c.EstudianteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
