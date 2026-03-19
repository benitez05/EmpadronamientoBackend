using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class RedSocialConfiguration : IEntityTypeConfiguration<RedSocial>
{
    public void Configure(EntityTypeBuilder<RedSocial> builder)
    {
        builder.ToTable("RedesSociales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Usuario)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.UrlPerfil)
            .HasMaxLength(500);

        builder.Property(x => x.TipoRedSocial)
            .HasMaxLength(100);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.RedesSociales)
            .HasForeignKey(x => x.PersonaId);
    }
}