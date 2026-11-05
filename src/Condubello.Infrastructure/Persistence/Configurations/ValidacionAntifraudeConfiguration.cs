using Condubello.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Condubello.Infrastructure.Persistence.Configurations;

public class ValidacionAntifraudeConfiguration : IEntityTypeConfiguration<ValidacionAntifraude>
{
    public void Configure(EntityTypeBuilder<ValidacionAntifraude> builder)
    {
        builder.ToTable("validaciones_antifraude");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.UrlFotoCapturada).HasMaxLength(500).IsRequired();

        builder.HasOne(v => v.Sesion)
            .WithMany(s => s.ValidacionesAntifraude)
            .HasForeignKey(v => v.SesionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
