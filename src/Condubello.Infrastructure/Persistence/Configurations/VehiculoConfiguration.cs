using Condubello.Domain.Entities;
using Condubello.Domain.Enums;
using Condubello.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("vehiculos");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Placa).HasMaxLength(10).IsRequired();
        builder.Property(v => v.Modelo).HasMaxLength(100).IsRequired();
        builder.Property(v => v.EstadoActual)
            .HasConversion(new UpperSnakeCaseEnumConverter<EstadoVehiculo>())
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique();
    }
}
