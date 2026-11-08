using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class RutaConfiguration : IEntityTypeConfiguration<Ruta>
{
    public void Configure(EntityTypeBuilder<Ruta> builder)
    {
        builder.ToTable("rutas");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(500);
        builder.Property(r => r.GeoJson).HasColumnType("jsonb");
        builder.HasIndex(r => r.Nombre).IsUnique();

        builder.HasMany(r => r.Sesiones)
            .WithOne(s => s.Ruta)
            .HasForeignKey(s => s.RutaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
