using BenitezLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BenitezLabs.Infrastructure.Persistence.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> builder)
    {
        builder.ToTable("Personas");

        builder.HasKey(x => x.Id);

        // Propiedades básicas
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ApellidoPaterno).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ApellidoMaterno).HasMaxLength(100);
        builder.Property(x => x.Apodo).HasMaxLength(100);
        builder.Property(x => x.Telefono).HasMaxLength(20);
        builder.Property(x => x.Sexo).HasMaxLength(20);
        builder.Property(x => x.Nacionalidad).HasMaxLength(50);

        // Relación con Organización (Multi-tenant)
        builder.HasOne(x => x.Organizacion)
            .WithMany()
            .HasForeignKey(x => x.OrganizacionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Fotos (Una persona tiene muchas fotos)
        builder.HasMany(x => x.Fotos)
            .WithOne(f => f.Persona)
            .HasForeignKey(f => f.IdPersona)
            .OnDelete(DeleteBehavior.Cascade);

        // Relaciones de Listas Existentes
        builder.HasMany(x => x.Direcciones)
            .WithOne(d => d.Persona)
            .HasForeignKey(d => d.PersonaId);

        builder.HasMany(x => x.RedesSociales)
            .WithOne(r => r.Persona)
            .HasForeignKey(r => r.PersonaId);

        builder.HasMany(x => x.Familiares)
            .WithOne(f => f.Persona)
            .HasForeignKey(f => f.PersonaId);

        // Relación Muchos a Muchos con Empadronamientos (vía tabla intermedia)
        builder.HasMany(x => x.Empadronamientos)
            .WithOne(ep => ep.Persona)
            .HasForeignKey(ep => ep.PersonaId);
            
        // Índices para búsquedas frecuentes
        builder.HasIndex(x => x.OrganizacionId);
        builder.HasIndex(x => new { x.Nombre, x.ApellidoPaterno, x.ApellidoMaterno });
    }
}