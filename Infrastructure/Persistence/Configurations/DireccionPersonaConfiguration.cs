using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class DireccionPersonaConfiguration : IEntityTypeConfiguration<DireccionPersona>
{
    public void Configure(EntityTypeBuilder<DireccionPersona> builder)
    {
        builder.ToTable("DireccionesPersona");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Calle).HasMaxLength(200);
        builder.Property(x => x.NumeroExterior).HasMaxLength(50);
        builder.Property(x => x.NumeroInterior).HasMaxLength(50);
        builder.Property(x => x.Colonia).HasMaxLength(150);
        builder.Property(x => x.Municipio).HasMaxLength(150);
        builder.Property(x => x.Estado).HasMaxLength(150);
        builder.Property(x => x.Pais).HasMaxLength(150);
        builder.Property(x => x.Referencia).HasMaxLength(500);
        builder.Property(x => x.ImagenUrl).HasMaxLength(500);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.Direcciones)
            .HasForeignKey(x => x.PersonaId);
    }
}