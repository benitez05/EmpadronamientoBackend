using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class FotoPersonaConfiguration : IEntityTypeConfiguration<FotoPersona>
{
    public void Configure(EntityTypeBuilder<FotoPersona> builder)
    {
        builder.ToTable("FotosPersona");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.TipoFotoNombre)
            .HasMaxLength(100);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.Fotos)
            .HasForeignKey(x => x.PersonaId);
    }
}