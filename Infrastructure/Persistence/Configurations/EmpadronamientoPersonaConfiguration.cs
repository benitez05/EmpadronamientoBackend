using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class EmpadronamientoPersonaConfiguration : IEntityTypeConfiguration<EmpadronamientoPersona>
{
    public void Configure(EntityTypeBuilder<EmpadronamientoPersona> builder)
    {
        builder.ToTable("EmpadronamientoPersonas");

        builder.HasKey(x => new { x.EmpadronamientoId, x.PersonaId });

        builder.Property(x => x.Observaciones)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Empadronamiento)
            .WithMany(e => e.Personas)
            .HasForeignKey(x => x.EmpadronamientoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Persona)
            .WithMany(p => p.Empadronamientos)
            .HasForeignKey(x => x.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}