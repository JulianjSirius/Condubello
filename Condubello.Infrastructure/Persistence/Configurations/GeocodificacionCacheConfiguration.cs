using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class GeocodificacionCacheConfiguration : IEntityTypeConfiguration<GeocodificacionCache>
{
    public void Configure(EntityTypeBuilder<GeocodificacionCache> builder)
    {
        builder.ToTable("geocodificacion_cache");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Clave).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Resultado).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(c => c.Clave).IsUnique();
    }
}
