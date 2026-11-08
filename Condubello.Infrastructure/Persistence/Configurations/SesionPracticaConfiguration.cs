using Condubello.Domain.Entities;
using Condubello.Domain.Enums;
using Condubello.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class SesionPracticaConfiguration : IEntityTypeConfiguration<SesionPractica>
{
    public void Configure(EntityTypeBuilder<SesionPractica> builder)
    {
        builder.ToTable("sesiones_practicas");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.DuracionHoras).HasPrecision(5, 2);
        builder.Property(s => s.EstadoPrueba)
            .HasConversion(new UpperSnakeCaseEnumConverter<EstadoPrueba>())
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(s => s.Estudiante)
            .WithMany(e => e.Sesiones)
            .HasForeignKey(s => s.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Instructor)
            .WithMany(i => i.Sesiones)
            .HasForeignKey(s => s.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
