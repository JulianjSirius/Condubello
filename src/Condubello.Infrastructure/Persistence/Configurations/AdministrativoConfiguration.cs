using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class AdministrativoConfiguration : IEntityTypeConfiguration<Administrativo>
{
    public void Configure(EntityTypeBuilder<Administrativo> builder)
    {
        builder.ToTable("administrativos");
        builder.ConfigurePersona();
        builder.Property(a => a.Cargo).HasMaxLength(100);
    }
}
