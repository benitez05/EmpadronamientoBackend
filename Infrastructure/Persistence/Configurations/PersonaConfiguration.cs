using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmpadronamientoBackend.Infrastructure.Persistence.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("Personas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).HasMaxLength(100);
        builder.Property(x => x.ApellidoPaterno).HasMaxLength(100);
        builder.Property(x => x.ApellidoMaterno).HasMaxLength(100);
        builder.Property(x => x.Apodo).HasMaxLength(100);
        builder.Property(x => x.Telefono).HasMaxLength(20);

        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Direcciones)
            .WithOne(d => d.Persona)
            .HasForeignKey(d => d.PersonaId);

        builder.HasMany(x => x.Fotos)
            .WithOne(f => f.Persona)
            .HasForeignKey(f => f.PersonaId);

        builder.HasMany(x => x.RedesSociales)
            .WithOne(r => r.Persona)
            .HasForeignKey(r => r.PersonaId);

        builder.HasMany(x => x.Familiares)
            .WithOne(f => f.Persona)
            .HasForeignKey(f => f.PersonaId);

        builder.HasMany(x => x.Rostros)
            .WithOne(r => r.Persona)
            .HasForeignKey(r => r.PersonaId);
    }
}