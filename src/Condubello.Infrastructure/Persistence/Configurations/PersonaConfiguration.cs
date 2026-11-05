using Condubello.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

internal static class PersonaConfiguration
{
    public static void ConfigurePersona<T>(this EntityTypeBuilder<T> builder) where T : Persona
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Cedula).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Apellidos).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(150).IsRequired();
        builder.Property(p => p.PasswordHash).IsRequired();

        builder.HasIndex(p => p.Cedula).IsUnique();
        builder.HasIndex(p => p.Email).IsUnique();
    }
}
