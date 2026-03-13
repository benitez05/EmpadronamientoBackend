using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class FamiliarConfiguration : IEntityTypeConfiguration<Familiar>
{
    public void Configure(EntityTypeBuilder<Familiar> builder)
    {
        builder.ToTable("Familiares");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NombreCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Telefono)
            .HasMaxLength(20);

        builder.Property(x => x.Direccion)
            .HasMaxLength(300);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.Familiares)
            .HasForeignKey(x => x.PersonaId);
    }
}